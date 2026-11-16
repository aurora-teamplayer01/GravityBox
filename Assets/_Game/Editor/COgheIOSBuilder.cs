using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace GravityBox.Editor
{
    /// <summary>Exports an existing COghe catalog for a physical iPhone or iPad.
    /// Signing credentials stay in the local environment, outside the repository.</summary>
    public static class COgheIOSBuilder
    {
        public const string Output="Builds/Venom/iOS";
        public const string BundleId="com.gravityboxlab.venom";

        [MenuItem("Gravity Box/COghe/Build iPhone Xcode project")]
        public static void Build() => BuildScenes(VenomCampaignBuilder.CampaignScenePaths(), Output);

        [MenuItem("Gravity Box/COghe/Spatial pilot/Build iPhone-iPad Xcode project · 65 levels")]
        public static void BuildSpatial() => BuildScenes(VenomCampaignBuilder.SpatialScenePaths(), "Builds/SpatialLab/iOS");

        private static void BuildScenes(string[] scenes, string output)
        {
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS,BuildTarget.iOS))
                throw new InvalidOperationException("Install iOS Build Support for this Unity version first.");
            foreach(var scene in scenes)if(!File.Exists(scene))throw new FileNotFoundException("Generate campaign before building",scene);

            ValidateServices();
            bool store=Environment.GetEnvironmentVariable("COGHE_STORE")=="1";
            bool simulator=Environment.GetEnvironmentVariable("COGHE_IOS_SIMULATOR")=="1";
            string releaseVersion=Environment.GetEnvironmentVariable("COGHE_IOS_VERSION") ?? "1.0.0";
            string releaseBuild=Environment.GetEnvironmentVariable("COGHE_IOS_BUILD") ?? "1";
            ValidateReleaseOptions(releaseVersion, releaseBuild, store,
                Environment.GetEnvironmentVariable("COGHE_BENCHMARK")=="1");
            if(simulator) output="Builds/SpatialLab/iOS-Simulator";
            var platform=NamedBuildTarget.iOS;
            string product=PlayerSettings.productName,identifier=PlayerSettings.GetApplicationIdentifier(platform);
            string oldVersion=PlayerSettings.bundleVersion, oldBuild=PlayerSettings.iOS.buildNumber;
            var sdk=PlayerSettings.iOS.sdkVersion;
            var backend=PlayerSettings.GetScriptingBackend(platform);
            var configuration=PlayerSettings.GetIl2CppCompilerConfiguration(platform);
            var orientation=PlayerSettings.defaultInterfaceOrientation;
            var device=PlayerSettings.iOS.targetDevice;
            try
            {
                PlayerSettings.productName=GravityBox.Venom.AlienPetBrand.DisplayName;
                PlayerSettings.SetApplicationIdentifier(platform,BundleId);
                PlayerSettings.iOS.sdkVersion=simulator?iOSSdkVersion.SimulatorSDK:iOSSdkVersion.DeviceSDK;
                PlayerSettings.bundleVersion=releaseVersion;
                PlayerSettings.iOS.buildNumber=releaseBuild;
                PlayerSettings.SetScriptingBackend(platform,ScriptingImplementation.IL2CPP);
                PlayerSettings.SetIl2CppCompilerConfiguration(platform,Il2CppCompilerConfiguration.Release);
                PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
                PlayerSettings.iOS.targetDevice=iOSTargetDevice.iPhoneAndiPad;
                Directory.CreateDirectory(output);
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                    scenes=scenes,target=BuildTarget.iOS,locationPathName=output,options=BuildOptions.None,extraScriptingDefines=COgheAndroidBuilder.Defines()});
                if(report.summary.result!=BuildResult.Succeeded)
                    throw new Exception("COghe iOS export failed: "+report.summary.result);
#if UNITY_IOS
                ConfigureSwiftResources(output);
                ConfigureSigning(output);
#endif
                Debug.Log("COGHE IOS EXPORT SUCCESS: "+scenes.Length+" scenes, "+Path.GetFullPath(output));
            }
            finally
            {
                PlayerSettings.productName=product;
                PlayerSettings.SetApplicationIdentifier(platform,identifier);
                PlayerSettings.iOS.sdkVersion=sdk;
                PlayerSettings.bundleVersion=oldVersion;
                PlayerSettings.iOS.buildNumber=oldBuild;
                PlayerSettings.SetScriptingBackend(platform,backend);
                PlayerSettings.SetIl2CppCompilerConfiguration(platform,configuration);
                PlayerSettings.defaultInterfaceOrientation=orientation;
                PlayerSettings.iOS.targetDevice=device;

            }
        }
        public static void ValidateReleaseOptions(string version, string build, bool store, bool benchmark)
        {
            if(!Regex.IsMatch(version ?? "", @"^\d+\.\d+\.\d+$"))
                throw new BuildFailedException("COGHE_IOS_VERSION must be major.minor.patch.");
            if(!int.TryParse(build, out int number) || number < 1)
                throw new BuildFailedException("COGHE_IOS_BUILD must be a positive integer.");
            if(store && benchmark) throw new BuildFailedException("Store builds cannot include benchmark automation.");
        }

        public static void ValidateFirebaseConfig(string xml)
        {
            var root=XDocument.Parse(xml).Root?.Element("dict");
            string Value(string key) => root?.Elements("key").FirstOrDefault(e=>e.Value==key)?.ElementsAfterSelf().FirstOrDefault()?.Value;
            if(Value("BUNDLE_ID")!=BundleId || Value("PROJECT_ID")!="coghe-57f21" ||
                Value("GOOGLE_APP_ID")!="1:446362971270:ios:8d705f7bc5e2ebbddc3507")
                throw new BuildFailedException("Firebase plist must match the registered Alien Pet iOS app.");
        }

        private static void ValidateServices()
        {
            const string config="Assets/GoogleService-Info.plist";
            if(!File.Exists(config)) throw new BuildFailedException("Missing iOS Firebase configuration: "+config);
            ValidateFirebaseConfig(File.ReadAllText(config));
            var settings=AssetDatabase.LoadMainAssetAtPath("Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset");
            if(settings==null || new SerializedObject(settings).FindProperty("adMobIOSAppId")?.stringValue!="ca-app-pub-7806638519709442~8888402391")
                throw new BuildFailedException("iOS AdMob App ID does not match Alien Pet iOS.");
        }
#if UNITY_IOS
        private static void ConfigureSwiftResources(string output)
        {
            const string script="copy-alienpet-ios-swift-resources.sh";
            File.Copy(Path.Combine("Tools",script),Path.Combine(output,script),true);
            string path=PBXProject.GetPBXProjectPath(output);
            var project=new PBXProject();project.ReadFromFile(path);
            string main=project.GetUnityMainTargetGuid();
            project.SetBuildProperty(main,"ENABLE_USER_SCRIPT_SANDBOXING","NO");
            project.AddShellScriptBuildPhase(main,"Alien Pet - Embed Firebase Swift resources", "/bin/sh",
                "/bin/sh \"$PROJECT_DIR/"+script+"\"");
            project.WriteToFile(path);
        }

        private static void ConfigureSigning(string output)
        {
            string team=Environment.GetEnvironmentVariable("COGHE_IOS_TEAM");
            string profile=Environment.GetEnvironmentVariable("COGHE_IOS_PROFILE");
            if(string.IsNullOrEmpty(team))return;
            bool manual=!string.IsNullOrEmpty(profile);
            string path=PBXProject.GetPBXProjectPath(output);
            var project=new PBXProject();project.ReadFromFile(path);
            string main=project.GetUnityMainTargetGuid(),framework=project.GetUnityFrameworkTargetGuid();
            foreach(string target in new[]{main,framework})
            {
                project.SetBuildProperty(target,"DEVELOPMENT_TEAM",team);
                project.SetBuildProperty(target,"CODE_SIGN_STYLE",manual?"Manual":"Automatic");
                project.SetBuildProperty(target,"CODE_SIGN_IDENTITY","Apple Development");
                project.SetBuildProperty(target,"PROVISIONING_PROFILE_SPECIFIER",manual&&target==main?profile:"");
            }
            project.WriteToFile(path);
        }
#endif
    }
}
