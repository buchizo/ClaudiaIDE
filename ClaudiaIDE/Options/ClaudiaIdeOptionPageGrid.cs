using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Shell;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.IO;
using ClaudiaIDE.Localized;

namespace ClaudiaIDE.Options
{
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[CLSCompliant(false), ComVisible(true)]
	[Guid("441f0a76-1771-41c2-817c-81b8b03fb0e8")]
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
            Extensions = ".png, .jpg";
            LoopSlideshow = true;
            MaxWidth = 0;
            MaxHeight = 0;
        }

        // TestCode
        [LocalManager.LocalizedCategory("Image")]
        [LocalManager.LocalizedDisplayName("BackgroundType")]
        [LocalManager.LocalizedDescription("BackgroundTypeDes")]
        // [Category("Image")]
        // [DisplayName("Image background type")]
        // [Description("Types of background image handling.")]
        [PropertyPageTypeConverter(typeof(ImageBackgroundTypeConverter))]
        [TypeConverter(typeof(ImageBackgroundTypeConverter))]
        public ImageBackgroundType ImageBackgroundType { get; set; }

        // [Category("Image")]
        [LocalManager.LocalizedCategoryAttribute("Image")]
        [DisplayName("Opacity")]
		[Description("Background image opacity. (value within the range of 0.00 <= 1.00)")]
		public double Opacity { get; set; }

        [Category("Layout")]
        [DisplayName("Horizontal Alignment")]
		[Description("Image position in horizon.")]
		[PropertyPageTypeConverter(typeof(PositionHTypeConverter))]
		[TypeConverter(typeof(PositionHTypeConverter))]
		public PositionH PositionHorizon { get; set; }

		[Category("Layout")]
		[DisplayName("Vertical Alignment")]
		[Description("Image position in vertical.")]
		[PropertyPageTypeConverter(typeof(PositionVTypeConverter))]
		[TypeConverter(typeof(PositionVTypeConverter))]
		public PositionV PositionVertical { get; set; }

		[Category("Slideshow")]
		[DisplayName("Directory Path")]
		[Description("Background image directory path.")]
        [EditorAttribute(typeof(BrowseDirectory), typeof(UITypeEditor))]
		public string BackgroundImageDirectoryAbsolutePath { get; set; }

        [Category("Slideshow")]
        [DisplayName("Update interval")]
        [Description("Background image change interval. (value in format: HH:mm:ss)")]
        [PropertyPageTypeConverter(typeof(TimeSpanConverter))]
        [TypeConverter(typeof(TimeSpanConverter))]
        public TimeSpan UpdateImageInterval { get; set; }

        [Category("Slideshow")]
        [DisplayName("Image animation interval")]
        [Description("Background image fade animation interval. (value in format: HH:mm:ss)")]
        [PropertyPageTypeConverter(typeof(TimeSpanConverter))]
        [TypeConverter(typeof(TimeSpanConverter))]
        public TimeSpan ImageFadeAnimationInterval { get; set; }

        [Category("Slideshow")]
        [DisplayName("Image extensions")]
        [Description("Only images with this extensions will be shown. (Comma separated)")]
        public string Extensions { get; set; }

        [Category("SingleImage")]
        [DisplayName("File Path")]
        [Description("Backgroud image file path.")]
        [EditorAttribute(typeof(BrowseFile), typeof(UITypeEditor))]
        public string BackgroundImageAbsolutePath { get; set; }

        [Category("Slideshow")]
        [DisplayName("Loop Slideshow")]
        [Description("This will cause the slideshow to loop back to the beginning after the last image has been shown.")]
        public bool LoopSlideshow { get; set; }

        [Category("Layout")]
        [DisplayName("Max Width")]
        [Description("Maximum width in pixels that the image can fill in the view.")]
        public int MaxWidth { get; set; }

        [Category("Layout")]
        [DisplayName("Max Height")]
        [Description("Maximum height in pixels that the image can fill in the view.")]
        public int MaxHeight { get; set; }

        protected override void OnApply(PageApplyEventArgs e)
        {
            try
            {
                Setting.Instance.OnApplyChanged();
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
            string str = value as string;

            if (str != null)
            {
                if (str == "Single") return ImageBackgroundType.Single;
                if (str == "Slideshow") return ImageBackgroundType.Slideshow;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string result = null;
                if ((int)value == 0)
                {
                    result = "Single";
                }
                else if ((int)value == 1)
                {
                    result = "Slideshow";
                }

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
			string str = value as string;

			if (str != null)
			{
				if (str == "Right") return PositionH.Right;
				if (str == "Left") return PositionH.Left;
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				string result = null;
				if ((int)value == 0)
				{
					result = "Left";
				}
				else if ((int)value == 1)
				{
					result = "Right";
				}

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
			string str = value as string;

			if (str != null)
			{
				if (str == "Top") return PositionV.Top;
				if (str == "Bottom") return PositionV.Bottom;
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				string result = null;
				if ((int)value == 0)
				{
					result = "Top";
				}
				else if ((int)value == 1)
				{
					result = "Bottom";
				}

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
            IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (edSvc != null)
            {
                var open = new FolderBrowserDialog();
                if (open.ShowDialog() == DialogResult.OK)
                {
                    return open.SelectedPath;
                }
            }
            return value;
        }
        public override bool GetPaintValueSupported(System.ComponentModel.ITypeDescriptorContext context)
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
            IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (edSvc != null)
            {
                OpenFileDialog open = new OpenFileDialog();
                open.FileName = Path.GetFileName((string)value);

                try
                {
                    open.InitialDirectory = Path.GetDirectoryName((string)value);
                }
                catch (Exception)
                {                    
                }

                if (open.ShowDialog() == DialogResult.OK)
                {
                    return open.FileName;
                }
            }
            return value;
        }
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }

}
