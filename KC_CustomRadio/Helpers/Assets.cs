using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Quriz.CustomRadio
{
    public static class Assets
    {
        private const string AssetBundleName = "radio_ui";
        
        public static GameObject CustomRadioUIPrefab;
        public static GameObject SongItemPrefab;
        
        public static void LoadAssets()
        {
            AssetBundle bundle = null;
            var assembly = Assembly.GetExecutingAssembly();
            var assetsAssembly = assembly.GetManifestResourceNames().First(x => x.Contains(AssetBundleName));
            using var stream = assembly.GetManifestResourceStream(assetsAssembly);
            using var memoryStream = new MemoryStream();
            stream!.CopyTo(memoryStream);
            bundle = AssetBundle.LoadFromMemory(memoryStream.ToArray());

            CustomRadioUIPrefab = bundle.LoadAsset<GameObject>("Custom Radio UI");
            CustomRadioUIPrefab.hideFlags = HideFlags.DontUnloadUnusedAsset;
            
            SongItemPrefab = bundle.LoadAsset<GameObject>("Song Item");
            SongItemPrefab.hideFlags = HideFlags.DontUnloadUnusedAsset;
        }
    }
}