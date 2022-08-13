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
                var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
                var path = dte?.Solution?.FileName;
                if (string.IsNullOrEmpty(path)) return null;

                // CMake or other directory project (not .sln) `path` is directory
                var dir = File.GetAttributes(path).HasFlag(FileAttributes.Directory)
                    ? path
                    : Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(dir)) return null;

                var configpath = Path.Combine(dir, ".claudiaideconfig");
                if (checkExists)
                    return File.Exists(configpath) ? configpath : null;
                return configpath;
            }
            catch
            {
                return null;
            }
        }
    }
}