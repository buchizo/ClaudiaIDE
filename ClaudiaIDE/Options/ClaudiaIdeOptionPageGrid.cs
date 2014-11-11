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

namespace ClaudiaIDE.Options
{
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[CLSCompliant(false), ComVisible(true)]
	[Guid("441f0a76-1771-41c2-817c-81b8b03fb0e8")]
	public class ClaudiaIdeOptionPageGrid : DialogPage
	{
		public ClaudiaIdeOptionPageGrid()
		{
			BackgroundImageAbsolutePath = "background.png";
			Opacity = 0.35;
			PositionHorizon = PositionH.Right;
			PositionVertical = PositionV.Bottom;
		}

		[Category("Image")]
		[DisplayName("File Path")]
		[Description("Background image file path.")]
        [EditorAttribute(typeof(BrowseFile), typeof(UITypeEditor))]
		public string BackgroundImageAbsolutePath { get; set; }

		[Category("Image")]
		[DisplayName("Opacity")]
		[Description("Background image opacity. (value within the range of 0.00 <= 1.00)")]
		public double Opacity { get; set; }

		[Category("Layout")]
		[DisplayName("Horizonal Alignment")]
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
                open.InitialDirectory = Path.GetDirectoryName((string)value);
                if (open.ShowDialog() == DialogResult.OK)
                {
                    return open.FileName;
                }
            }
            return value;
        }
        public override bool GetPaintValueSupported(System.ComponentModel.ITypeDescriptorContext context)
        {
            return false;
        }
    }

}
