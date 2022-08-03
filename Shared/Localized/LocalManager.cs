using System;
using System.ComponentModel;
using System.Resources;

namespace ClaudiaIDE.Localized
{
    internal class LocalManager
    {
        internal static ResourceManager _rm;

        private static ResourceManager GetInstance()
        {
            if (_rm == null) _rm = ResLocalized.ResourceManager;

            return _rm;
        }

        [AttributeUsage(AttributeTargets.Assembly |
                        AttributeTargets.Module |
                        AttributeTargets.Class |
                        AttributeTargets.Struct |
                        AttributeTargets.Enum |
                        AttributeTargets.Constructor |
                        AttributeTargets.Method |
                        AttributeTargets.Property |
                        AttributeTargets.Field |
                        AttributeTargets.Event |
                        AttributeTargets.Interface |
                        AttributeTargets.Parameter |
                        AttributeTargets.Delegate |
                        AttributeTargets.ReturnValue |
                        AttributeTargets.GenericParameter)]
        internal class LocalizedDescriptionAttribute : DescriptionAttribute
        {
            internal LocalizedDescriptionAttribute(string _key) : base(Localize(_key))
            {
            }

            private static string Localize(string _key)
            {
                return GetInstance().GetString(_key);
            }
        }

        [AttributeUsage(AttributeTargets.All)]
        internal class LocalizedCategoryAttribute : CategoryAttribute
        {
            internal LocalizedCategoryAttribute(string _key) : base(Localize(_key))
            {
            }

            private static string Localize(string _key)
            {
                return GetInstance().GetString(_key);
            }
        }

        [AttributeUsage(AttributeTargets.Class |
                        AttributeTargets.Method |
                        AttributeTargets.Property |
                        AttributeTargets.Event)]
        internal class LocalizedDisplayNameAttribute : DisplayNameAttribute
        {
            internal LocalizedDisplayNameAttribute(string _key) : base(Localize(_key))
            {
            }

            private static string Localize(string _key)
            {
                return GetInstance().GetString(_key);
            }
        }
    }
}