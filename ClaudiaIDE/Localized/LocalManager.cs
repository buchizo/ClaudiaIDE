using System;
using System.ComponentModel;
using System.Resources;

namespace ClaudiaIDE.Localized
{
    internal class LocalManager
    {
        internal static ResourceManager _rm = null;

        private static ResourceManager GetInstance()
        {
            if (_rm == null)
            {
                _rm = ResLocalized.ResourceManager;
            }

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
            private static string Localize(string _key)
            {
                return GetInstance().GetString(_key);
            }

            internal LocalizedDescriptionAttribute(string _key) : base(Localize(_key))
            {
            }
        }

        [AttributeUsage(AttributeTargets.All)]
        internal class LocalizedCategoryAttribute : CategoryAttribute
        {
            private static string Localize(string _key)
            {
                return GetInstance().GetString(_key);
            }

            internal LocalizedCategoryAttribute(string _key) : base(Localize(_key))
            {
            }
        }

        [AttributeUsage(AttributeTargets.Class |
                        AttributeTargets.Method |
                        AttributeTargets.Property |
                        AttributeTargets.Event)]
        internal class LocalizedDisplayNameAttribute : DisplayNameAttribute
        {
            private static string Localize(string _key)
            {
                return GetInstance().GetString(_key);
            }

            internal LocalizedDisplayNameAttribute(string _key) : base(Localize(_key))
            {
            }
        }
    }
}