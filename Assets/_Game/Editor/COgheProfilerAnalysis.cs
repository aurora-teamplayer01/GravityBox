using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;

namespace GravityBox.Editor
{
    /// <summary>Offline analysis of an actual Unity Profiler capture; never starts gameplay or changes saves.</summary>
    public static class COgheProfilerAnalysis
    {
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        private sealed class Job { public string input, output; }
        private static readonly Queue<Job> Pending = new Queue<Job>();
        private static bool processing;
        private static int jobCount, failedJobs;

        [Serializable] private sealed class Summary
        {
            public string input, unity, exportedUtc, analyzer;
            public int firstFrame, lastFrame, frames, timedFrames, threadFrames, invalidSamples, negativeSelfSamples, gcEventsWithoutSize;
            public double meanFrameMs, p50FrameMs, p95FrameMs, p99FrameMs, maxFrameMs;
            public long gcAllocBytes;
            public string interpretation = "Frame indexes are zero-based ProfilerDriver indexes (UI uses +1). " +
                "Frame times are main-thread frame elapsed time, not CPU busy time. " +
                "Inclusive marker totals overlap ancestors and must not be added. " +
                "Self time subtracts immediate child durations only; negative values are retained and counted. " +
                "Thread frame times overlap other threads. GC bytes sum known direct GC.Alloc Size metadata, not parent attribution; " +
                "a nonzero missing-size count makes that byte total an incomplete lower bound. Missing GPU timings are blank. " +
                "Zero-duration boundary frames remain in CSV but are excluded from the distribution (timedFrames). " +
                "Capture boundaries, waits and loading remain in these unfiltered exports; choose the intended range in Profile Analyzer.";
        }

        private sealed class Marker
        {
            public string thread, name;
            public ulong threadId;
            public long calls, gcBytes;
            public int frames, lastFrame = -1, gcEvents, missingGcSize;
            public double inclusiveMs, selfMs, maxCallMs;
        }

        [MenuItem("Gravity Box/COghe/Profiler/Load and Analyze Capture")]
        public static void LoadAndAnalyze()
        {
            string input = Environment.GetEnvironmentVariable("COGHE_PROFILE_INPUT");
            if (string.IsNullOrEmpty(input) && !Application.isBatchMode)
                input = EditorUtility.OpenFilePanel("Open Unity Profiler capture", "", "raw,data");
            if (string.IsNullOrEmpty(input)) throw new ArgumentException("Set COGHE_PROFILE_INPUT to a Unity .raw or .data capture.");
            input = Path.GetFullPath(input);
            if (!File.Exists(input)) throw new FileNotFoundException("Profiler capture does not exist", input);
            string output = Environment.GetEnvironmentVariable("COGHE_PROFILE_OUTPUT");
            if (string.IsNullOrEmpty(output)) output = Path.Combine(Path.GetDirectoryName(input), Path.GetFileNameWithoutExtension(input) + "-analysis");
            Analyze(input, Path.GetFullPath(output));
        }

        public static void Analyze(string input, string output)
        {
            StartJobs(new[] { new Job { input = Path.GetFullPath(input), output = Path.GetFullPath(output) } });
        }

        [MenuItem("Gravity Box/COghe/Profiler/Analyze Capture Directory")]
        public static void LoadAndAnalyzeDirectory()
        {
            string directory = Environment.GetEnvironmentVariable("COGHE_PROFILE_DIRECTORY");
            if (string.IsNullOrEmpty(directory) && !Application.isBatchMode)
                directory = EditorUtility.OpenFolderPanel("Folder containing Unity .raw captures", "", "");
            if (string.IsNullOrEmpty(directory)) throw new ArgumentException("Set COGHE_PROFILE_DIRECTORY to the folder containing .raw captures.");
            directory = Path.GetFullPath(directory);
            string[] files = Directory.GetFiles(directory, "*.raw", SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.Ordinal);
            if (files.Length == 0) throw new FileNotFoundException("No .raw captures in " + directory);
            var jobs = new List<Job>();
            foreach (string file in files)
                jobs.Add(new Job { input = file, output = Path.Combine(directory, "analysis", Path.GetFileNameWithoutExtension(file)) });
            StartJobs(jobs);
        }

        private static void StartJobs(IEnumerable<Job> jobs)
        {
            if (processing) throw new InvalidOperationException("Profiler analysis is already running; wait for COGHE_PROFILE_ANALYSIS_DONE.");
            foreach (var job in jobs) Pending.Enqueue(job);
            jobCount = Pending.Count; failedJobs = 0; processing = true;
            ProfilerDriver.enabled = false;
            var profilerType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProfilerWindow");
            if (profilerType != null) EditorWindow.GetWindow(profilerType).Show();
            // Let the real Profiler window finish OnEnable before loading the first capture.
            EditorApplication.delayCall += ProcessNext;
        }

        private static void ProcessNext()
        {
            if (Pending.Count == 0)
            {
                processing = false;
                Debug.Log("COGHE_PROFILE_ANALYSIS_DONE captures=" + jobCount + " failed=" + failedJobs);
                return;
            }
            Job job = Pending.Dequeue();
            try
            {
                ProfilerDriver.enabled = false;
                // Present but internal in Unity 6000.3; keep the utility compatible with its public assembly API.
                var setHistory = typeof(ProfilerDriver).GetMethod("SetMaxFrameHistoryLength", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (setHistory == null) throw new MissingMethodException(typeof(ProfilerDriver).FullName, "SetMaxFrameHistoryLength");
                setHistory.Invoke(null, new object[] { 2000 });
                if (!ProfilerDriver.LoadProfile(job.input, false)) throw new InvalidDataException("Unity could not load profiler capture: " + job.input);
                int first = ProfilerDriver.firstFrameIndex, last = ProfilerDriver.lastFrameIndex;
                if (first < 0 || last < first) throw new InvalidDataException("The capture contains no profiler frames.");
                Directory.CreateDirectory(job.output);
                var summary = new Summary { input = job.input, unity = Application.unityVersion, exportedUtc = DateTime.UtcNow.ToString("O"), firstFrame = first, lastFrame = last };
                Export(job.output, summary);
                // Do not replace ProfilerDriver's data until the corresponding Analyzer pull has completed.
                ShowAnalyzer(first, last, job.output, status =>
                {
                    try
                    {
                        summary.analyzer = status;
                        File.WriteAllText(Path.Combine(job.output, "summary.json"), JsonUtility.ToJson(summary, true));
                        Debug.Log("COGHE_PROFILE_ANALYZED " + job.output + " frames=" + summary.frames + " analyzer=" + status);
                    }
                    catch (Exception exception) { failedJobs++; Debug.LogError("COGHE_PROFILE_ANALYSIS_FAILED " + job.input + " " + exception); }
                    finally { EditorApplication.delayCall += ProcessNext; }
                });
            }
            catch (Exception exception)
            {
                failedJobs++;
                Debug.LogError("COGHE_PROFILE_ANALYSIS_FAILED " + job.input + " " + exception);
                EditorApplication.delayCall += ProcessNext;
            }
        }

        private static void Export(string output, Summary summary)
        {
            var totals = new Dictionary<string, Marker>();
            var frameTimes = new List<double>();
            using (var frames = new StreamWriter(Path.Combine(output, "frames.csv")))
            using (var threadFrames = new StreamWriter(Path.Combine(output, "thread-frames.csv")))
            using (var samples = new StreamWriter(Path.Combine(output, "marker-frames.csv")))
            {
                frames.WriteLine("frame,uiFrame,startMs,frameMs,gpuMs,gpuAvailability");
                threadFrames.WriteLine("frame,threadIndex,threadId,thread,startMs,threadFrameMs,samples,gcAllocBytes,gcEventsWithoutSize");
                samples.WriteLine("frame,threadId,thread,marker,calls,inclusiveMs,selfMs,maxCallMs,gcAllocBytes,gcEventsWithoutSize");
                for (int frame = summary.firstFrame; frame <= summary.lastFrame; frame++)
                {
                    for (int threadIndex = 0; ; threadIndex++)
                    using (var data = ProfilerDriver.GetRawFrameDataView(frame, threadIndex))
                    {
                        if (!data.valid) break;
                        string thread = string.IsNullOrEmpty(data.threadGroupName) ? data.threadName : data.threadGroupName + "/" + data.threadName;
                        if (threadIndex == 0)
                        {
                            if (data.frameTimeMs > 0) frameTimes.Add(data.frameTimeMs);
                            string gpu = data.frameGpuTimeMs > 0 ? F(data.frameGpuTimeMs) : "";
                            frames.WriteLine(string.Join(",", frame, frame + 1, F(data.frameStartTimeMs), F(data.frameTimeMs), gpu, Csv(ProfilerDriver.GetGpuStatisticsAvailabilityState(frame).ToString())));
                            summary.frames++;
                        }
                        var perFrame = new Dictionary<string, Marker>();
                        long gcBytes = 0; int missingGc = 0;
                        // Sample zero is the thread root, not a CPU marker (same convention as Profile Analyzer).
                        for (int sample = 1; sample < data.sampleCount; sample++)
                        {
                            string name = data.GetSampleName(sample);
                            double inclusive = data.GetSampleTimeMs(sample);
                            if (inclusive < 0 || double.IsNaN(inclusive) || double.IsInfinity(inclusive)) { summary.invalidSamples++; continue; }
                            double self = SelfTime(data, sample, inclusive);
                            if (self < -.0001) summary.negativeSelfSamples++;
                            long allocation = 0; bool knownAllocation = true, isGc = name == "GC.Alloc";
                            if (isGc)
                            {
                                knownAllocation = TryAllocationSize(data, sample, out allocation);
                                if (!knownAllocation) missingGc++;
                                else gcBytes += allocation;
                            }
                            if (!perFrame.TryGetValue(name, out var marker))
                                perFrame.Add(name, marker = new Marker { name = name, thread = thread, threadId = data.threadId });
                            marker.calls++; marker.inclusiveMs += inclusive; marker.selfMs += self; marker.maxCallMs = Math.Max(marker.maxCallMs, inclusive);
                            marker.gcBytes += allocation;
                            if (isGc) { marker.gcEvents++; if (!knownAllocation) marker.missingGcSize++; }
                        }
                        threadFrames.WriteLine(string.Join(",", frame, threadIndex, data.threadId, Csv(thread), F(data.frameStartTimeMs), F(data.frameTimeMs), data.sampleCount, gcBytes, missingGc));
                        summary.threadFrames++; summary.gcAllocBytes += gcBytes; summary.gcEventsWithoutSize += missingGc;
                        foreach (var marker in perFrame.Values)
                        {
                            samples.WriteLine(string.Join(",", frame, marker.threadId, Csv(marker.thread), Csv(marker.name), marker.calls, F(marker.inclusiveMs), F(marker.selfMs), F(marker.maxCallMs), marker.gcBytes, marker.missingGcSize));
                            string key = marker.threadId.ToString(Invariant) + "\n" + marker.thread + "\n" + marker.name;
                            if (!totals.TryGetValue(key, out var total)) totals.Add(key, total = new Marker { name = marker.name, thread = marker.thread, threadId = marker.threadId });
                            total.calls += marker.calls; total.inclusiveMs += marker.inclusiveMs; total.selfMs += marker.selfMs;
                            total.maxCallMs = Math.Max(total.maxCallMs, marker.maxCallMs); total.gcBytes += marker.gcBytes;
                            total.gcEvents += marker.gcEvents; total.missingGcSize += marker.missingGcSize;
                            if (total.lastFrame != frame) { total.frames++; total.lastFrame = frame; }
                        }
                    }
                }
            }
            var ordered = new List<Marker>(totals.Values);
            ordered.Sort((a, b) => b.selfMs.CompareTo(a.selfMs));
            using (var writer = new StreamWriter(Path.Combine(output, "markers.csv")))
            {
                writer.WriteLine("threadId,thread,marker,framesPresent,calls,inclusiveMs,selfMs,meanInclusiveMsPerPresentFrame,meanSelfMsPerPresentFrame,maxCallMs,gcAllocBytes,gcEvents,gcEventsWithoutSize");
                foreach (var m in ordered)
                    writer.WriteLine(string.Join(",", m.threadId, Csv(m.thread), Csv(m.name), m.frames, m.calls, F(m.inclusiveMs), F(m.selfMs), F(m.inclusiveMs / m.frames), F(m.selfMs / m.frames), F(m.maxCallMs), m.gcBytes, m.gcEvents, m.missingGcSize));
            }
            if (frameTimes.Count == 0) return;
            summary.timedFrames = frameTimes.Count;
            double sum = 0; foreach (double value in frameTimes) sum += value;
            frameTimes.Sort(); summary.meanFrameMs = sum / frameTimes.Count;
            summary.p50FrameMs = Percentile(frameTimes, .50); summary.p95FrameMs = Percentile(frameTimes, .95);
            summary.p99FrameMs = Percentile(frameTimes, .99); summary.maxFrameMs = frameTimes[frameTimes.Count - 1];
        }

        private static double SelfTime(RawFrameDataView data, int sample, double inclusive)
        {
            // Depth-first sample order: jump over each direct child's entire subtree.
            int child = sample + 1;
            for (int n = 0; n < data.GetSampleChildrenCount(sample); n++)
            {
                if (child >= data.sampleCount) throw new InvalidDataException("Profiler child index exceeds the thread sample buffer.");
                inclusive -= data.GetSampleTimeMs(child);
                child += 1 + data.GetSampleChildrenCountRecursive(child);
            }
            return inclusive;
        }

        private static bool TryAllocationSize(RawFrameDataView data, int sample, out long bytes)
        {
            bytes = 0;
            var metadata = data.GetMarkerMetadataInfo(data.GetSampleMarkerId(sample));
            for (int i = 0; i < metadata.Length && i < data.GetSampleMetadataCount(sample); i++)
                if (metadata[i].name == "Size" && metadata[i].unit == Unity.Profiling.ProfilerMarkerDataUnit.Bytes)
                { bytes = data.GetSampleMetadataAsLong(sample, i); return true; }
            return false;
        }

        private static void ShowAnalyzer(int first, int last, string output, Action<string> completed, int attempt = 0, double deadline = 0)
        {
            try
            {
                if (deadline == 0) deadline = EditorApplication.timeSinceStartup + 30;
                var profilerType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProfilerWindow");
                if (profilerType != null) EditorWindow.GetWindow(profilerType).Show();
                Type type = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    if ((type = assembly.GetType("UnityEditor.Performance.ProfileAnalyzer.ProfileAnalyzerWindow")) != null) break;
                if (type == null) { failedJobs++; completed("Profile Analyzer package unavailable; CSV export completed."); return; }
                var window = EditorWindow.GetWindow(type); window.Show();
                object bridge = Field(type, "m_ProfilerWindowInterface").GetValue(window);
                bool ready = bridge != null && (bool)Method(bridge.GetType(), "IsReady").Invoke(bridge, null);
                bool busy = (bool)Method(type, "IsAnalysisRunning").Invoke(window, null);
                if (!ready || busy)
                {
                    if (EditorApplication.timeSinceStartup >= deadline) throw new InvalidOperationException("Profile Analyzer did not become ready for the capture within 30 seconds");
                    // GetProfilerWindowHandle itself schedules initialization for the next Editor delayCall.
                    // Call it only once: resetting it on every poll would prevent readiness permanently.
                    if (!ready && attempt == 0 && bridge != null)
                        Method(bridge.GetType(), "GetProfilerWindowHandle").Invoke(bridge, null);
                    EditorApplication.delayCall += () => ShowAnalyzer(first, last, output, completed, attempt + 1, deadline);
                    return;
                }
                var activeTab = Field(type, "m_ActiveTab");
                activeTab.SetValue(window, Enum.Parse(activeTab.FieldType, "Summary"));
                object view = Field(type, "m_ProfileSingleView").GetValue(window);
                object graph = Field(type, "m_FrameTimeGraph").GetValue(window);
                Method(type, "PullFromProfiler").Invoke(window, new[] { (object)(first + 1), last + 1, view, graph });
                object data = view.GetType().GetField("data", Members).GetValue(view);
                string pdata = Path.Combine(output, "capture.pdata");
                bool saved = (bool)data.GetType().GetMethod("Save", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { (object)pdata, data });
                if (!saved) throw new IOException("Profile Analyzer could not save capture.pdata.");
                // Invoke the same synchronous analysis and completion path used by the package's background worker.
                Field(type, "m_FullAnalysisRequired").SetValue(window, true);
                Method(type, "AnalyzeSync").Invoke(window, null);
                var completion = Method(type, "CompleteAsyncOperation");
                completion.Invoke(window, new[] { Enum.Parse(completion.GetParameters()[0].ParameterType, "AnalyzeDone") });
                window.Repaint();
                completed("Pulled, analyzed and saved capture.pdata with installed Profile Analyzer; its existing UI filters may apply. CSV exports are unfiltered.");
            }
            catch (Exception exception)
            {
                string message = exception.GetBaseException().Message;
                Debug.LogWarning("Profile Analyzer automatic pull unavailable: " + message + ". Use Pull Data in its open window; raw CSV export remains available.");
                failedJobs++;
                completed("Automatic Profile Analyzer pull failed: " + message);
            }
        }

        private static FieldInfo Field(Type type, string name) => type.GetField(name, Members) ?? throw new MissingFieldException(type.FullName, name);
        private static MethodInfo Method(Type type, string name) => type.GetMethod(name, Members) ?? throw new MissingMethodException(type.FullName, name);
        private static double Percentile(List<double> values, double q) => values[Math.Min(values.Count - 1, (int)(values.Count * q))];
        private static string F(double value) => value.ToString("R", Invariant);
        private static string Csv(string value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
    }
}
