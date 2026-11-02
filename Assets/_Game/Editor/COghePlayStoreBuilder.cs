using System;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GravityBox.Editor
{
    /// <summary>Signed Play upload bundle. Credentials are supplied only to this process, never saved in the project.</summary>
    public static class COghePlayStoreBuilder
    {
        private static readonly string[] SigningVariables = {
            "ALIEN_PET_KEYSTORE", "ALIEN_PET_KEY_ALIAS", "ALIEN_PET_STORE_PASSWORD", "ALIEN_PET_KEY_PASSWORD"
        };
        private static readonly string[] ForbiddenDefines = {
            "COGHE_TEST_TOOLS", "COGHE_MOBILE_BENCHMARK", "DEVELOPMENT_BUILD", "UNITY_ASSERTIONS"
        };

        public static void Build()
        {
            string keyPath = null, keyAlias = null, storePassword = null, keyPassword = null;
            try
            {
                keyPath = Required(SigningVariables[0]);
                keyAlias = Required(SigningVariables[1]);
                storePassword = Required(SigningVariables[2]);
                keyPassword = Required(SigningVariables[3]);
                ClearSigningEnvironment();
                BuildSigned(keyPath, keyAlias, storePassword, keyPassword);
            }
            finally
            {
                ClearSigningEnvironment();
                keyPath = keyAlias = storePassword = keyPassword = null;
            }
        }

        private static void BuildSigned(string keyPath, string keyAlias, string storePassword, string keyPassword)
        {
            if (Environment.GetEnvironmentVariable("COGHE_STORE") != "1" ||
                Environment.GetEnvironmentVariable("COGHE_BENCHMARK") == "1" ||
                Environment.GetEnvironmentVariable("COGHE_PROFILE") == "1")
                throw new BuildFailedException("Play bundles require store mode with benchmark and profiling disabled.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
                throw new BuildFailedException("Android Build Support is required.");
            if (!File.Exists(keyPath)) throw new BuildFailedException("The upload keystore is missing.");
            var scenes = VenomCampaignBuilder.SpatialScenePaths();
            foreach (var scene in scenes)
                if (!File.Exists(scene)) throw new FileNotFoundException("A catalog scene is missing.", scene);

            var platform = NamedBuildTarget.Android;
            string defines = PlayerSettings.GetScriptingDefineSymbols(platform);
            if (defines.Split(';').Any(d => ForbiddenDefines.Contains(d.Trim())))
                throw new BuildFailedException("Remove test, benchmark, development, or assertion defines before a Play build.");
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(COgheAppIconBuilder.IconPath);
            if (icon == null || !PlayerSettings.GetIcons(NamedBuildTarget.Unknown, IconKind.Any).Contains(icon))
                throw new BuildFailedException("Apply the approved app icon before a Play build.");
            foreach (var kind in new[] { AndroidPlatformIconKind.Legacy, AndroidPlatformIconKind.Round })
                if (PlayerSettings.GetPlatformIcons(platform, kind).Any(slot => slot.GetTexture() != icon))
                    throw new BuildFailedException("Android launcher icons must use the approved artwork.");

            string output = Path.GetFullPath(Required("ALIEN_PET_AAB_OUTPUT"));
            if (!output.EndsWith(".aab", StringComparison.OrdinalIgnoreCase))
                throw new BuildFailedException("Play output must end in .aab.");
            string version = Required("ALIEN_PET_VERSION");
            if (!int.TryParse(Required("ALIEN_PET_VERSION_CODE"), out int versionCode) || versionCode < 1)
                throw new BuildFailedException("A positive Play version code is required.");

            string product = PlayerSettings.productName, identifier = PlayerSettings.GetApplicationIdentifier(platform);
            string oldVersion = PlayerSettings.bundleVersion;
            int oldVersionCode = PlayerSettings.Android.bundleVersionCode;
            var backend = PlayerSettings.GetScriptingBackend(platform);
            var configuration = PlayerSettings.GetIl2CppCompilerConfiguration(platform);
            var architectures = PlayerSettings.Android.targetArchitectures;
            var minSdk = PlayerSettings.Android.minSdkVersion;
            var targetSdk = PlayerSettings.Android.targetSdkVersion;
            var orientation = PlayerSettings.defaultInterfaceOrientation;
            bool customKey = PlayerSettings.Android.useCustomKeystore;
            string oldKeyName = PlayerSettings.Android.keystoreName, oldKeyAlias = PlayerSettings.Android.keyaliasName;
            string oldStorePassword = PlayerSettings.Android.keystorePass, oldKeyPassword = PlayerSettings.Android.keyaliasPass;
            bool bundle = EditorUserBuildSettings.buildAppBundle, export = EditorUserBuildSettings.exportAsGoogleAndroidProject;
            bool development = EditorUserBuildSettings.development, debugging = EditorUserBuildSettings.allowDebugging;
            bool profiler = EditorUserBuildSettings.connectProfiler, deepProfile = EditorUserBuildSettings.buildWithDeepProfilingSupport;
            try
            {
                PlayerSettings.productName = AlienPetBrand.DisplayName;
                PlayerSettings.SetApplicationIdentifier(platform, COgheAndroidBuilder.BundleId);
                PlayerSettings.bundleVersion = version;
                PlayerSettings.Android.bundleVersionCode = versionCode;
                PlayerSettings.SetScriptingBackend(platform, ScriptingImplementation.IL2CPP);
                PlayerSettings.SetIl2CppCompilerConfiguration(platform, Il2CppCompilerConfiguration.Release);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
                PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = keyPath;
                PlayerSettings.Android.keyaliasName = keyAlias;
                PlayerSettings.Android.keystorePass = storePassword;
                PlayerSettings.Android.keyaliasPass = keyPassword;
                EditorUserBuildSettings.buildAppBundle = true;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                EditorUserBuildSettings.development = false;
                EditorUserBuildSettings.allowDebugging = false;
                EditorUserBuildSettings.connectProfiler = false;
                EditorUserBuildSettings.buildWithDeepProfilingSupport = false;
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes = scenes, target = BuildTarget.Android, locationPathName = output,
                    options = BuildOptions.None, extraScriptingDefines = Array.Empty<string>()
                });
                if (report.summary.result != BuildResult.Succeeded)
                    throw new BuildFailedException("Play bundle build failed: " + report.summary.result);
                if ((report.summary.options & (BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler)) != 0)
                    throw new BuildFailedException("Unexpected development option in the Play bundle.");
                Debug.Log($"ALIEN PET PLAY BUNDLE SUCCESS: {output}; version={version} ({versionCode}); scenes={scenes.Length}; ARM64 IL2CPP Release.");
            }
            finally
            {
                PlayerSettings.Android.keystorePass = oldStorePassword;
                PlayerSettings.Android.keyaliasPass = oldKeyPassword;
                PlayerSettings.Android.keystoreName = oldKeyName;
                PlayerSettings.Android.keyaliasName = oldKeyAlias;
                PlayerSettings.Android.useCustomKeystore = customKey;
                PlayerSettings.productName = product;
                PlayerSettings.SetApplicationIdentifier(platform, identifier);
                PlayerSettings.bundleVersion = oldVersion;
                PlayerSettings.Android.bundleVersionCode = oldVersionCode;
                PlayerSettings.SetScriptingBackend(platform, backend);
                PlayerSettings.SetIl2CppCompilerConfiguration(platform, configuration);
                PlayerSettings.Android.targetArchitectures = architectures;
                PlayerSettings.Android.minSdkVersion = minSdk;
                PlayerSettings.Android.targetSdkVersion = targetSdk;
                PlayerSettings.defaultInterfaceOrientation = orientation;
                EditorUserBuildSettings.buildAppBundle = bundle;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = export;
                EditorUserBuildSettings.development = development;
                EditorUserBuildSettings.allowDebugging = debugging;
                EditorUserBuildSettings.connectProfiler = profiler;
                EditorUserBuildSettings.buildWithDeepProfilingSupport = deepProfile;
                AssetDatabase.SaveAssets();
                storePassword = keyPassword = oldStorePassword = oldKeyPassword = null;
            }
        }

        private static string Required(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value)) throw new BuildFailedException("Missing required build environment: " + name);
            return value;
        }

        private static void ClearSigningEnvironment()
        {
            foreach (string name in SigningVariables) Environment.SetEnvironmentVariable(name, null);
        }
    }
}
