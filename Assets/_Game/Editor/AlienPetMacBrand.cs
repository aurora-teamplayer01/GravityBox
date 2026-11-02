using System.IO;
using System.Xml;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.Callbacks;

namespace GravityBox.Editor
{
    /// <summary>Rename the public Mac bundle without moving Unity's existing PlayerPrefs/save namespace.</summary>
    public static class AlienPetMacBrand
    {
        [PostProcessBuild(100)]
        public static void Apply(BuildTarget target, string path)
        {
            if (target != BuildTarget.StandaloneOSX ||
                !(path.Contains("COgheProduct/") || path.Contains("SpatialLab/"))) return;
            var plist = Path.Combine(path, "Contents/Info.plist");
            var document = new XmlDocument { XmlResolver = null };
            document.Load(plist);
            // Mono's XML writer adds an empty internal DTD subset that Apple's plist reader rejects.
            if (document.DocumentType != null) document.RemoveChild(document.DocumentType);
            var dict = document.SelectSingleNode("/plist/dict");
            foreach (var key in new[] { "CFBundleDisplayName", "CFBundleName" })
            {
                var node = dict.SelectSingleNode("key[.='" + key + "']");
                if (node == null)
                {
                    node = document.CreateElement("key"); node.InnerText = key; dict.AppendChild(node);
                    var value = document.CreateElement("string"); dict.AppendChild(value);
                }
                node.NextSibling.InnerText = AlienPetBrand.DisplayName;
            }
            document.Save(plist);
        }
    }
}
