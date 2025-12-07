using System.Reflection;
using System.Resources;

namespace Shared.Helpers
{
    public static class ResourceExtension
    {
        private static ResourceManager Resources = new ResourceManager("ClaudiaIDE.Localized.ResLocalized", Assembly.GetExecutingAssembly());

        public static string GetResourceString(string key)
        {
            try
            {
                return Resources.GetString(key) ?? "";
            }
            catch
            {
                return "";
            }
        }
    }
}
