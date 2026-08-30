using System;
using System.IO;
using HWorld.Core.World;

namespace HWorld.Example
{
    internal static class WorldFileService
    {
        public static void Save(World world, string path)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("A file path is required.", nameof(path));

            var fullPath = Path.GetFullPath(path);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var json = WorldSerializer.Serialize(world);
            File.WriteAllText(fullPath, json);
        }

        public static World Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("A file path is required.", nameof(path));
            var json = File.ReadAllText(Path.GetFullPath(path));
            return WorldSerializer.Deserialize(json);
        }
    }
}
