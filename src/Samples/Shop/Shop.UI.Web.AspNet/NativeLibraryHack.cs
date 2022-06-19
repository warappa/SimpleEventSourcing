using System;
using System.Diagnostics;
using System.IO;

namespace Shop.UI.Web
{
    // https://stackoverflow.com/questions/43811731/unable-to-load-dll-e-sqlite3-the-specified-module-could-not-be-found
    public class NativeLibraryHack
    {
        public static bool Hacked { get; private set; }

        public static bool DoHack()
        {
            if (Hacked) return true;

            try
            {
                const string runtimeFolderName = "/runtimes";

                var destinationPath = typeof(SQLitePCL.raw).Assembly.Location
                    .Replace("\\", "/");
                var destinationLength = destinationPath.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                var destinationDirectory = destinationPath.Substring(0, destinationLength) + runtimeFolderName;

                var sourcePath = new Uri(typeof(SQLitePCL.raw).Assembly.CodeBase)
                    .AbsolutePath;
                var sourceLength = sourcePath.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                var sourceDirectory = sourcePath.Substring(0, sourceLength) + runtimeFolderName;

                if (Directory.Exists(sourceDirectory))
                    CopyFilesRecursively(new DirectoryInfo(sourceDirectory), new DirectoryInfo(destinationDirectory));
            }
            catch (Exception ex)
            {
                //Ignore Exception
                Debug.WriteLine(ex.Message);
                return false;
            }

            return (Hacked = true);
        }

        private static void CopyFilesRecursively(
            DirectoryInfo source,
            DirectoryInfo target
        )
        {
            foreach (var dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));

            foreach (var file in source.GetFiles())
            {
                try
                {
                    var destinationFile = Path.Combine(target.FullName, file.Name);
                    if (!File.Exists(destinationFile))
                        file.CopyTo(destinationFile);
                }
                catch (Exception ex)
                {
                    //Ignore Exception
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
