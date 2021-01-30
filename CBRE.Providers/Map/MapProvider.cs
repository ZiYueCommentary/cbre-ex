﻿using CBRE.DataStructures.MapObjects;
using CBRE.Providers.Texture;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace CBRE.Providers.Map {
    public abstract class MapProvider {
        private static readonly List<MapProvider> RegisteredProviders;

        public static string warnings;

        static MapProvider() {
            RegisteredProviders = new List<MapProvider>();
        }

        public static void Register(MapProvider provider) {
            RegisteredProviders.Add(provider);
        }

        public static void Deregister(MapProvider provider) {
            RegisteredProviders.Remove(provider);
        }

        public static void DeregisterAll() {
            RegisteredProviders.Clear();
        }

        public static DataStructures.MapObjects.Map GetMapFromFile(string fileName, IEnumerable<string> modelDirs, out Image[] lightmaps) {
            if (!File.Exists(fileName)) throw new ProviderException("The supplied file doesn't exist.");
            var provider = RegisteredProviders.FirstOrDefault(p => p.IsValidForFileName(fileName));
            if (provider != null) {
                warnings = "";
                return provider.GetFromFile(fileName, modelDirs, out lightmaps);
            }
            throw new ProviderNotFoundException("No map provider was found for this file.");
        }

        public static void SaveMapToFile(string filename, DataStructures.MapObjects.Map map, DataStructures.GameData.GameData gameData, TextureCollection textureCollection = null) {
            var provider = RegisteredProviders.FirstOrDefault(p => p.IsValidForFileName(filename));
            if (provider != null) {
                provider.SaveToFile(filename, map, gameData, textureCollection);
                return;
            }
            throw new ProviderNotFoundException("No map provider was found for this file format.");
        }

        public static IEnumerable<MapFeature> GetFormatFeatures(string filename) {
            var provider = RegisteredProviders.FirstOrDefault(p => p.IsValidForFileName(filename));
            if (provider != null) {
                return provider.GetFormatFeatures();
            }
            throw new ProviderNotFoundException("No map provider was found for this file format.");
        }

        protected virtual DataStructures.MapObjects.Map GetFromFile(string filename, IEnumerable<string> modelDirs, out Image[] lightmaps) {
            using (var strm = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
                return GetFromStream(strm, modelDirs, out lightmaps);
            }
        }

        protected virtual void SaveToFile(string filename, DataStructures.MapObjects.Map map, DataStructures.GameData.GameData gameData, TextureCollection textureCollection) {
            using (var strm = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
                SaveToStream(strm, map, gameData, textureCollection);
            }
        }

        protected abstract bool IsValidForFileName(string filename);
        protected abstract DataStructures.MapObjects.Map GetFromStream(Stream stream, IEnumerable<string> modelDirs, out Image[] lightmaps);
        protected abstract void SaveToStream(Stream stream, DataStructures.MapObjects.Map map, DataStructures.GameData.GameData gameData, TextureCollection textureCollection);
        protected abstract IEnumerable<MapFeature> GetFormatFeatures();
    }
}
