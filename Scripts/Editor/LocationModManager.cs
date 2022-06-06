using DaggerfallWorkshop.Game.Utility.ModSupport;
using FullSerializer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LocationLoader
{
#if UNITY_EDITOR
    public static class LocationModManager
    {
        static Dictionary<string, ModInfo> DevModInfo;
        static Dictionary<string, string> DevModNameToDirectory;

        static Dictionary<string, ModInfo> PackagedModInfo;
        static Dictionary<string, AssetBundle> PackagedModBundle;

        static void LoadDevModInfos()
        {
            if (DevModInfo != null)
                return;

            DevModInfo = new Dictionary<string, ModInfo>(StringComparer.OrdinalIgnoreCase);
            DevModNameToDirectory = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            string modsfolder = Path.Combine(Application.dataPath, "Game", "Mods");
            foreach (var directory in Directory.EnumerateDirectories(modsfolder))
            {
                string foundFile = Directory.EnumerateFiles(directory).FirstOrDefault(file => file.EndsWith(".dfmod.json"));
                if (string.IsNullOrEmpty(foundFile))
                    continue;

                ModInfo modInfo = null;
                if (ModManager._serializer.TryDeserialize(fsJsonParser.Parse(File.ReadAllText(foundFile)), ref modInfo).Failed)
                    continue;

                DevModInfo.Add(modInfo.ModTitle, modInfo);
                DevModNameToDirectory.Add(modInfo.ModTitle, directory);
            }
        }

        public static ModInfo GetDevModInfo(string modName)
        {
            LoadDevModInfos();
            return DevModInfo[modName];
        }

        public static string GetDevModDirectory(string modName)
        {
            LoadDevModInfos();
            return DevModNameToDirectory[modName];
        }

        public static IEnumerable<string> GetDevMods()
        {
            LoadDevModInfos();
            return DevModInfo.Keys;
        }

        public static string GetDevModAssetPath(string modName, string assetName)
        {
            ModInfo modInfo = GetDevModInfo(modName);
            if (modInfo == null)
                return null;

            return modInfo.Files.FirstOrDefault(file => Path.GetFileNameWithoutExtension(file) == assetName);
        }

        public static void LoadPackagedMods()
        {
            if (PackagedModInfo != null)
                return;

            PackagedModInfo = new Dictionary<string, ModInfo>(StringComparer.OrdinalIgnoreCase);
            PackagedModBundle = new Dictionary<string, AssetBundle>(StringComparer.OrdinalIgnoreCase);

            foreach (string file in Directory.EnumerateFiles(Path.Combine(Application.dataPath, "StreamingAssets", "Mods"), "*.dfmod"))
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(file);
                if (bundle == null)
                    continue;

                string dfmodAssetName = bundle.GetAllAssetNames().FirstOrDefault(assetName => assetName.EndsWith(".dfmod.json"));
                if (string.IsNullOrEmpty(dfmodAssetName))
                    continue;

                TextAsset dfmodAsset = bundle.LoadAsset<TextAsset>(dfmodAssetName);
                if (dfmodAsset == null)
                    continue;

                ModInfo modInfo = null;
                if (ModManager._serializer.TryDeserialize(fsJsonParser.Parse(dfmodAsset.text), ref modInfo).Failed)
                    continue;

                PackagedModInfo.Add(modInfo.ModTitle, modInfo);
                PackagedModBundle.Add(modInfo.ModTitle, bundle);
            }
        }

        public static bool IsPackagedMod(string modName)
        {
            LoadPackagedMods();
            return PackagedModInfo.ContainsKey(modName);
        }

        public static ModInfo GetPackagedModInfo(string modName)
        {
            LoadPackagedMods();
            return PackagedModInfo[modName];
        }

        public static AssetBundle GetPackagedModBundle(string modName)
        {
            LoadPackagedMods();
            return PackagedModBundle[modName];
        }

        public static IEnumerable<string> GetPackagedMods()
        {
            LoadPackagedMods();
            return PackagedModInfo.Keys;
        }

        public static ModInfo GetModInfo(string modName)
        {
            if (IsPackagedMod(modName))
            {
                return GetPackagedModInfo(modName);
            }
            else
            {
                return GetDevModInfo(modName);
            }
        }

        static Regex MakeFilePattern(string filePattern)
        {
            if (string.IsNullOrEmpty(filePattern))
                return null;

            var lowerPattern = filePattern.ToLower();

            var regexPattern = Regex.Escape(lowerPattern).Replace("\\?", ".").Replace("\\*", ".*");
            return new Regex(regexPattern, RegexOptions.Compiled);
        }

        public static IEnumerable<T> FindAssets<T>(string modName, Regex fileMatch) where T : UnityEngine.Object
        {
            var modInfo = GetModInfo(modName);
            if(modInfo == null)
                return Enumerable.Empty<T>();

            var files = modInfo.Files.Where(file => fileMatch.IsMatch(Path.GetFileName(file)));

            if(IsPackagedMod(modName))
            {
                AssetBundle assetBundle = GetPackagedModBundle(modName);
                return files.Select(file => assetBundle.LoadAsset<T>(Path.GetFileName(file)));
            }
            else
            {
                return files.Select(file => AssetDatabase.LoadAssetAtPath<T>(file));
            }
        }

        public static IEnumerable<T> FindAssets<T>(string modName, string filePattern) where T : UnityEngine.Object
        {
            return FindAssets<T>(modName, MakeFilePattern(filePattern));
        }
    }
#endif
}
