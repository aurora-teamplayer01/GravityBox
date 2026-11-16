using System;
using UnityEditor;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif
using UnityEditor.Build;
using UnityEngine;

namespace GravityBox.Editor
{
    /// <summary>Applies the approved C artwork without changing the in-game creature.</summary>
    public static class COgheAppIconBuilder
    {
        public const string IconPath = "Assets/_Game/Branding/COghe-AppIcon.png";

        [MenuItem("Gravity Box/COghe/Apply approved app icon C")]
        public static void Apply()
        {
            var importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing COghe app icon: " + IconPath);

            // Launcher artwork is exported by Unity at each required size. It is
            // outside Resources so it does not become an in-game texture dependency.
            importer.textureType = TextureImporterType.Default;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (texture == null || texture.width != texture.height)
                throw new InvalidOperationException("COghe app icon must be a square texture.");

            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { texture }, IconKind.Any);
#if UNITY_ANDROID   // Android Build Support is optional on an iOS-only Mac; switch to Android to refresh these slots
            SetAndroidIcons(AndroidPlatformIconKind.Legacy, texture);
            SetAndroidIcons(AndroidPlatformIconKind.Round, texture);
#else
            Debug.LogWarning("COGHE APP ICON: active target is not Android; Android legacy/round slots left unchanged.");
#endif
            // C is an approved, flattened illustration. Do not invent a two-layer
            // adaptive crop: Android uses these complete legacy/round alternatives.
            AssetDatabase.SaveAssets();
            Debug.Log("COGHE APP ICON: approved C applied to default and Android legacy/round slots.");
        }

        private static void SetAndroidIcons(PlatformIconKind kind, Texture2D texture)
        {
            var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
            foreach (var icon in icons) icon.SetTexture(texture);
            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
        }
    }
}
