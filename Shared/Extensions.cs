using System;
using System.Collections.Generic;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace ClaudiaIDE
{
    public static class Extensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            var rng = new Random();
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public static class VisualStudioUtility
    {
        public static string GetSolutionSettingsFileFullPath(bool checkExists = true)
        {
            try
            {
                if (!TryGetSolutionPath(out string path)) return null;

                // CMake or other directory project (not .sln) `path` is directory
                var dir = File.GetAttributes(path).HasFlag(FileAttributes.Directory)
                    ? path
                    : Path.GetDirectoryName(path);
                if (string.IsNullOrWhiteSpace(dir)) return null;

                var configPath = Path.Combine(dir, ".claudiaideconfig");
                return checkExists
                    ? File.Exists(configPath) ? configPath : null
                    : configPath;
            }
            catch
            {
                return null;
            }
        }


        public static bool TryGetSolutionPath(out string path)
        {
            try
            {
                var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
                path = dte?.Solution?.FileName;
                return !string.IsNullOrWhiteSpace(path);
            }
            catch
            {
                path = null;
                return false;
            }
        }
    }
}