using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace GravityBox.Editor
{
    /// <summary>Native defaults must be closed before Unity's managed age gate can run.</summary>
    public sealed class COghePrivacyBuild : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 2000;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            if (PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) != COgheAndroidBuilder.BundleId) return;
            string file = Path.Combine(path, "src/main/AndroidManifest.xml");
            var manifest = XDocument.Load(file);
            ConfigureAndroidManifest(manifest);
            manifest.Save(file);
        }

        public static void ConfigureAndroidManifest(XDocument manifest)
        {
            XNamespace android = "http://schemas.android.com/apk/res/android";
            XNamespace tools = "http://schemas.android.com/tools";
            var root = manifest.Root;
            if (root == null || root.Name.LocalName != "manifest") throw new BuildFailedException("Missing Android manifest root.");
            root.SetAttributeValue(XNamespace.Xmlns + "tools", tools.NamespaceName);
            var app = root.Element("application");
            if (app == null) { app = new XElement("application"); root.Add(app); }
            void Metadata(string name, bool value)
            {
                var node = app.Elements("meta-data").FirstOrDefault(e => (string)e.Attribute(android + "name") == name);
                if (node == null) { node = new XElement("meta-data", new XAttribute(android + "name", name)); app.Add(node); }
                node.SetAttributeValue(android + "value", value ? "true" : "false");
                node.SetAttributeValue(tools + "replace", "android:value");
            }
            Metadata("firebase_data_collection_default_enabled", false);
            Metadata("firebase_analytics_collection_enabled", false);
            Metadata("google_analytics_adid_collection_enabled", false);
            Metadata("google_analytics_default_allow_analytics_storage", false);
            Metadata("google_analytics_default_allow_ad_storage", false);
            Metadata("google_analytics_default_allow_ad_user_data", false);
            Metadata("google_analytics_default_allow_ad_personalization_signals", false);
            Metadata("com.google.android.gms.ads.DELAY_APP_MEASUREMENT_INIT", true);
            foreach (string name in new[] { "com.google.firebase.provider.FirebaseInitProvider", "com.google.android.gms.ads.MobileAdsInitProvider" })
            {
                foreach (var existing in app.Elements("provider").Where(e => (string)e.Attribute(android + "name") == name).ToArray()) existing.Remove();
                app.Add(new XElement("provider", new XAttribute(android + "name", name), new XAttribute(tools + "node", "remove")));
            }
            // Neither children nor adults need advertising IDs for this game's contextual ads.
            foreach (string name in new[] { "com.google.android.gms.permission.AD_ID", "android.permission.ACCESS_ADSERVICES_AD_ID", "android.permission.ACCESS_ADSERVICES_ATTRIBUTION", "android.permission.ACCESS_ADSERVICES_TOPICS" })
            {
                foreach (var existing in root.Elements("uses-permission").Where(e => (string)e.Attribute(android + "name") == name).ToArray()) existing.Remove();
                root.Add(new XElement("uses-permission", new XAttribute(android + "name", name), new XAttribute(tools + "node", "remove")));
            }
        }

        [PostProcessBuild(2000)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
#if UNITY_IOS
            if (target != BuildTarget.iOS || PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS) != COgheIOSBuilder.BundleId) return;
            string file = Path.Combine(path, "Info.plist");
            var plist = new PlistDocument(); plist.ReadFromFile(file);
            foreach (string key in new[] { "FirebaseDataCollectionDefaultEnabled", "FIREBASE_ANALYTICS_COLLECTION_ENABLED", "GOOGLE_ANALYTICS_IDFA_COLLECTION_ENABLED", "GOOGLE_ANALYTICS_IDFV_COLLECTION_ENABLED", "GOOGLE_ANALYTICS_DEFAULT_ALLOW_ANALYTICS_STORAGE", "GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_STORAGE", "GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_USER_DATA", "GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_PERSONALIZATION_SIGNALS" }) plist.root.SetBoolean(key, false);
            plist.root.SetBoolean("GADDelayAppMeasurementInit", true);
            plist.WriteToFile(file);
#endif
        }
    }
}
