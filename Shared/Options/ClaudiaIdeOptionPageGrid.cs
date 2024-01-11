using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using ClaudiaIDE.Localized;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ClaudiaIDE.Options
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid(GuidList.OptionPageId)]
    public class ClaudiaIdeOptionPageGrid : DialogPage
    {
        public ClaudiaIdeOptionPageGrid()
        {
            BackgroundImageAbsolutePath = "Images\\background.png";
            BackgroundImageDirectoryAbsolutePath = "Images";
            Opacity = 0.35;
            PositionHorizon = PositionH.Right;
            PositionVertical = PositionV.Bottom;
            UpdateImageInterval = TimeSpan.FromMinutes(1);
            ImageFadeAnimationInterval = TimeSpan.FromSeconds(5);
            Extensions = ".png, .jpg, .gif, .bmp";
            LoopSlideshow = true;
            MaxWidth = 0;
            MaxHeight = 0;
            SoftEdgeX = 0;
            SoftEdgeY = 0;
            BlurRadius = 0;
            BlurMethod = ImageBlurMethod.Gaussian;
            ImageStretch = ImageStretch.None;
            ExpandToIDE = false;
            ViewBoxPointX = 0;
            ViewBoxPointY = 0;
            IsLimitToMainlyEditorWindow = false;
            ViewPortHeight = 1;
            ViewPortWidth = 1;
            ViewPortPointX = 0;
            ViewPortPointY = 0;
            TileMode = TileMode.None;
            SingleWebUrl = "";
            WebApiApiEndpoint = "";
            WebApiJsonKey = "";
            WebApiDownloadInterval = TimeSpan.FromMinutes(5);
            IsTransparentToContentMargin = false;
            IsTransparentToStickyScroll = false;
        }

        [LocalManager.LocalizedCategoryAttribute("Image")]
        [LocalManager.LocalizedDisplayNameAttribute("BackgroundType")]
        [LocalManager.LocalizedDescriptionAttribute("BackgroundTypeDes")]
        [PropertyPageTypeConverter(typeof(ImageBackgroundTypeConverter))]
        [TypeConverter(typeof(ImageBackgroundTypeConverter))]
        public ImageBackgroundType ImageBackgroundType { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Image")]
        [LocalManager.LocalizedDisplayNameAttribute("OpacityType")]
        [LocalManager.LocalizedDescriptionAttribute("OpacityTypeDes")]
        public double Opacity { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("HorizontalAlignmentType")]
        [LocalManager.LocalizedDescriptionAttribute("HorizontalAlignmentTypeDes")]
        [PropertyPageTypeConverter(typeof(PositionHTypeConverter))]
        [TypeConverter(typeof(PositionHTypeConverter))]
        public PositionH PositionHorizon { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("VerticalAlignmentType")]
        [LocalManager.LocalizedDescriptionAttribute("VerticalAlignmentTypeDes")]
        [PropertyPageTypeConverter(typeof(PositionVTypeConverter))]
        [TypeConverter(typeof(PositionVTypeConverter))]
        public PositionV PositionVertical { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Slideshow")]
        [LocalManager.LocalizedDisplayNameAttribute("DirectoryPathType")]
        [LocalManager.LocalizedDescriptionAttribute("DirectoryPathTypeDes")]
        [EditorAttribute(typeof(BrowseDirectory), typeof(UITypeEditor))]
        public string BackgroundImageDirectoryAbsolutePath { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Slideshow")]
        [LocalManager.LocalizedDisplayNameAttribute("UpdateIntervalType")]
        [LocalManager.LocalizedDescriptionAttribute("UpdateIntervalTypeDes")]
        [PropertyPageTypeConverter(typeof(TimeSpanConverter))]
        [TypeConverter(typeof(TimeSpanConverter))]
        public TimeSpan UpdateImageInterval { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Slideshow")]
        [LocalManager.LocalizedDisplayNameAttribute("ImageAnimationIntervalType")]
        [LocalManager.LocalizedDescriptionAttribute("ImageAnimationIntervalTypeDes")]
        [PropertyPageTypeConverter(typeof(TimeSpanConverter))]
        [TypeConverter(typeof(TimeSpanConverter))]
        public TimeSpan ImageFadeAnimationInterval { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Slideshow")]
        [LocalManager.LocalizedDisplayNameAttribute("ImageExtensionsType")]
        [LocalManager.LocalizedDescriptionAttribute("ImageExtensionsTypeDes")]
        public string Extensions { get; set; }

        [LocalManager.LocalizedCategoryAttribute("SingleImage")]
        [LocalManager.LocalizedDisplayNameAttribute("FilePathType")]
        [LocalManager.LocalizedDescriptionAttribute("FilePathTypeDes")]
        [EditorAttribute(typeof(BrowseFile), typeof(UITypeEditor))]
        public string BackgroundImageAbsolutePath { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Slideshow")]
        [LocalManager.LocalizedDisplayNameAttribute("LoopSlideshowType")]
        [LocalManager.LocalizedDescriptionAttribute("LoopSlideshowTypeDes")]
        public bool LoopSlideshow { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Slideshow")]
        [LocalManager.LocalizedDisplayNameAttribute("ShuffleSlideshowType")]
        [LocalManager.LocalizedDescriptionAttribute("ShuffleSlideshowTypeDes")]
        public bool ShuffleSlideshow { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Scope")]
        [LocalManager.LocalizedDisplayNameAttribute("ExpandToIDEType")]
        [LocalManager.LocalizedDescriptionAttribute("ExpandToIDETypeDes")]
        public bool ExpandToIDE { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("MaxWidthType")]
        [LocalManager.LocalizedDescriptionAttribute("MaxWidthTypeDes")]
        public int MaxWidth { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("MaxHeightType")]
        [LocalManager.LocalizedDescriptionAttribute("MaxHeightTypeDes")]
        public int MaxHeight { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("SoftEdgeX")]
        [LocalManager.LocalizedDescriptionAttribute("SoftEdgeDes")]
        public int SoftEdgeX { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("SoftEdgeY")]
        [LocalManager.LocalizedDescriptionAttribute("SoftEdgeDes")]
        public int SoftEdgeY { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("BlurRadius")]
        [LocalManager.LocalizedDescriptionAttribute("BlurRadiusDes")]
        public int BlurRadius { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("BlurMethod")]
        [LocalManager.LocalizedDescriptionAttribute("BlurMethodDes")]
        public ImageBlurMethod BlurMethod { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("ImageStretchType")]
        [LocalManager.LocalizedDescriptionAttribute("ImageStretchTypeDes")]
        [PropertyPageTypeConverter(typeof(ImageStretchTypeConverter))]
        [TypeConverter(typeof(ImageStretchTypeConverter))]
        public ImageStretch ImageStretch { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("ViewBoxPointX")]
        [LocalManager.LocalizedDescriptionAttribute("ViewBoxPointXDes")]
        public double ViewBoxPointX { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("ViewBoxPointY")]
        [LocalManager.LocalizedDescriptionAttribute("ViewBoxPointYDes")]
        public double ViewBoxPointY { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Scope")]
        [LocalManager.LocalizedDisplayNameAttribute("IsLimitToMainlyEditorWindow")]
        [LocalManager.LocalizedDescriptionAttribute("IsLimitToMainlyEditorWindowDes")]
        public bool IsLimitToMainlyEditorWindow { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("ViewPortWidth")]
        [LocalManager.LocalizedDescriptionAttribute("ViewPortWidthDes")]
        public double ViewPortWidth { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("ViewPortHeight")]
        [LocalManager.LocalizedDescriptionAttribute("ViewPortHeightDes")]
        public double ViewPortHeight { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("ViewPortPointX")]
        [LocalManager.LocalizedDescriptionAttribute("ViewPortPointXDes")]
        public double ViewPortPointX { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("ViewPortPointY")]
        [LocalManager.LocalizedDescriptionAttribute("ViewPortPointYDes")]
        public double ViewPortPointY { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Layout")]
        [LocalManager.LocalizedDisplayNameAttribute("TileMode")]
        [LocalManager.LocalizedDescriptionAttribute("TileModeDes")]
        [PropertyPageTypeConverter(typeof(TileModeConverter))]
        [TypeConverter(typeof(TileModeConverter))]
        public TileMode TileMode { get; set; }

        [LocalManager.LocalizedCategoryAttribute("WebImage")]
        [LocalManager.LocalizedDisplayNameAttribute("Url")]
        [LocalManager.LocalizedDescriptionAttribute("UrlDescription")]
        public string SingleWebUrl { get; set; }

        [LocalManager.LocalizedCategoryAttribute("WebApi")]
        [LocalManager.LocalizedDisplayNameAttribute("ApiEndpoint")]
        [LocalManager.LocalizedDescriptionAttribute("ApiEndpointDescription")]
        public string WebApiApiEndpoint { get; set; }

        [LocalManager.LocalizedCategoryAttribute("WebApi")]
        [LocalManager.LocalizedDisplayNameAttribute("JsonKey")]
        [LocalManager.LocalizedDescriptionAttribute("JsonKeyDescription")]
        public string WebApiJsonKey { get; set; }

        [LocalManager.LocalizedCategoryAttribute("WebApi")]
        [LocalManager.LocalizedDisplayNameAttribute("WebApiDownloadInterval")]
        [LocalManager.LocalizedDescriptionAttribute("WebApiDownloadIntervalDescpription")]
        [PropertyPageTypeConverter(typeof(TimeSpanConverter))]
        [TypeConverter(typeof(TimeSpanConverter))]
        public TimeSpan WebApiDownloadInterval { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Scope")]
        [LocalManager.LocalizedDisplayNameAttribute("IsTransparentToContentMarginType")]
        [LocalManager.LocalizedDescriptionAttribute("IsTransparentToContentMarginTypeDes")]
        public bool IsTransparentToContentMargin { get; set; }

        [LocalManager.LocalizedCategoryAttribute("Scope")]
        [LocalManager.LocalizedDisplayNameAttribute("IsTransparentToStickyScrollType")]
        [LocalManager.LocalizedDescriptionAttribute("IsTransparentToStickyScrollTypeDes")]
        public bool IsTransparentToStickyScroll { get; set; }

        protected override void OnApply(PageApplyEventArgs e)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                Setting.DefaultInstance.OnApplyChanged();
            }
            catch
            {
            }

            base.OnApply(e);
        }
    }

    public class ImageBackgroundTypeConverter : EnumConverter
    {
        public ImageBackgroundTypeConverter()
            : base(typeof(ImageBackgroundType))
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str)
            {
                if (str == "Single") return ImageBackgroundType.Single;
                if (str == "Slideshow") return ImageBackgroundType.Slideshow;
                if (str == "SingleEach") return ImageBackgroundType.SingleEach;
                if (str == "SingleWeb") return ImageBackgroundType.WebSingle;
                if (str == "WebApi") return ImageBackgroundType.WebApi;
                return ImageBackgroundType.Single;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string result = null;
                if ((int)value == 0)
                    result = "Single";
                else if ((int)value == 1)
                    result = "Slideshow";
                else if ((int)value == 2)
                    result = "SingleEach";
                else if ((int)value == 3)
                    result = "SingleWeb";
                else if ((int)value == 4) result = "WebApi";
                if (result != null) return result;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class PositionHTypeConverter : EnumConverter
    {
        public PositionHTypeConverter()
            : base(typeof(PositionH))
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str)
            {
                if (str == "Right") return PositionH.Right;
                if (str == "Left") return PositionH.Left;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string result = null;
                if ((int)value == 0)
                    result = "Left";
                else if ((int)value == 1) result = "Right";

                if (result != null) return result;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class PositionVTypeConverter : EnumConverter
    {
        public PositionVTypeConverter()
            : base(typeof(PositionV))
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str)
            {
                if (str == "Top") return PositionV.Top;
                if (str == "Bottom") return PositionV.Bottom;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string result = null;
                if ((int)value == 0)
                    result = "Top";
                else if ((int)value == 1) result = "Bottom";

                if (result != null) return result;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class ImageStretchTypeConverter : EnumConverter
    {
        public ImageStretchTypeConverter()
            : base(typeof(ImageStretch))
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str)
            {
                if (str == "None") return ImageStretch.None;
                if (str == "Uniform") return ImageStretch.Uniform;
                if (str == "UniformToFill") return ImageStretch.UniformToFill;
                if (str == "Fill") return ImageStretch.Fill;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string result = null;
                if ((int)value == 0)
                    result = "None";
                else if ((int)value == 1)
                    result = "Uniform";
                else if ((int)value == 2)
                    result = "UniformToFill";
                else if ((int)value == 3) result = "Fill";

                if (result != null) return result;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class TileModeConverter : EnumConverter
    {
        public TileModeConverter()
            : base(typeof(TileMode))
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str)
            {
                if (str == "None") return TileMode.None;
                if (str == "Tile") return TileMode.Tile;
                if (str == "FlipX") return TileMode.FlipX;
                if (str == "FlipY") return TileMode.FlipY;
                if (str == "FlipXY") return TileMode.FlipXY;
                return TileMode.None;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string result = null;
                if ((int)value == 0)
                    result = "None";
                else if ((int)value == 1)
                    result = "FlipX";
                else if ((int)value == 2)
                    result = "FlipY";
                else if ((int)value == 3)
                    result = "FlipXY";
                else if ((int)value == 4) result = "Tile";

                if (result != null) return result;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    internal class BrowseDirectory : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (edSvc != null)
            {
                var open = new CommonOpenFileDialog { IsFolderPicker = true };
                if (open.ShowDialog() == CommonFileDialogResult.Ok) return open.FileName;
            }

            return value;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }

    internal class BrowseFile : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (edSvc != null)
            {
                var open = new OpenFileDialog
                {
                    FileName = Path.GetFileName((string)value)
                };

                try
                {
                    open.InitialDirectory = Path.GetDirectoryName((string)value);
                }
                catch (Exception)
                {
                }

                if (open.ShowDialog() == DialogResult.OK) return open.FileName;
            }

            return value;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }

    public class UseColorThemeTypeConverter : EnumConverter
    {
        public UseColorThemeTypeConverter()
            : base(typeof(UseColorThemeType))
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str)
            {
                if (str == "Light") return UseColorThemeType.Light;
                if (str == "Dark") return UseColorThemeType.Dark;
                if (str == "UseSystemSetting") return UseColorThemeType.UseSystemSetting;
                else return UseColorThemeType.Light;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string result = "Light";
                if ((int)value == 0)
                    result = "Light";
                else if ((int)value == 1)
                    result = "Dark";
                else if ((int)value == 2)
                    result = "UseSystemSetting";

                if (result != null) return result;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid(GuidList.DarkThemeOptionPageId)]
    public class ClaudiaIdeDarkThemeOptionPageGrid : ClaudiaIdeOptionPageGrid
    {
        public ClaudiaIdeDarkThemeOptionPageGrid() : base()
        {
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                Setting.DefaultInstance.OnApplyChanged();
            }
            catch
            {
            }

            base.OnApply(e);
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid(GuidList.GeneralOptionPageId)]
    public class ClaudiaIdeGeneralOptionPageGrid : DialogPage
    {
        [LocalManager.LocalizedCategoryAttribute("General")]
        [LocalManager.LocalizedDisplayNameAttribute("UseColorTheme")]
        [LocalManager.LocalizedDescriptionAttribute("UseColorThemeDescription")]
        [PropertyPageTypeConverter(typeof(UseColorThemeTypeConverter))]
        [TypeConverter(typeof(UseColorThemeTypeConverter))]
        public UseColorThemeType UseColorTheme { get; set; }

        public ClaudiaIdeGeneralOptionPageGrid()
        {
            UseColorTheme = UseColorThemeType.Light;
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                Setting.DefaultInstance.OnApplyChanged();
            }
            catch
            {
            }

            base.OnApply(e);
        }
    }
}