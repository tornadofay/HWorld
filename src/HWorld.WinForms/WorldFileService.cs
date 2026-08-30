using System;
using System.IO;
using HWorld.Core.World;

namespace HWorld.WinForms
{
    public static class WorldFileService
    {
        public static string DefaultWorldDirectory
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Worlds"); }
        }

        public static string PrepareDefaultWorldDirectory()
        {
            var directory = DefaultWorldDirectory;
            Directory.CreateDirectory(directory);
            return directory;
        }

        public static void Save(World world, string path)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("A file path is required.", nameof(path));

            var fullPath = Path.GetFullPath(path);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, WorldSerializer.Serialize(world));
        }

        public static World Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("A file path is required.", nameof(path));
            return WorldSerializer.Deserialize(File.ReadAllText(Path.GetFullPath(path)));
        }
    }
}
