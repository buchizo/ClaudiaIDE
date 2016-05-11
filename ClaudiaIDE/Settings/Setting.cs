using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;

namespace ClaudiaIDE.Settings
{
	public class Setting
	{
        private static readonly Setting instance = new Setting();
        private static readonly string CONFIGFILE = "config.txt";

        internal System.IServiceProvider ServiceProvider { get; set; }

        public event EventHandler OnChanged;

        public static Setting Instance
        {
            get
            {
                return instance;
            }
        }

        public Setting()
		{
			var assemblylocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			BackgroundImagesDirectoryAbsolutePath = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, "Images");
            BackgroundImageAbsolutePath = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, "background.png");
            Opacity = 0.35;
			PositionHorizon = PositionH.Right;
			PositionVertical = PositionV.Bottom;
		    UpdateImageInterval = TimeSpan.FromMinutes(30);
		    Extensions = ".png, .jpg";
            ImageBackgroundType = ImageBackgroundType.Single;
            LoopSlideshow = true;
            MaxWidth = 0;
            MaxHeight = 0;
			ImageZoomType = ZoomType.Zoom;
		}

        public ImageBackgroundType ImageBackgroundType { get; set; }
        public double Opacity { get; set; }
		public PositionV PositionVertical { get; set; }
		public PositionH PositionHorizon { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
		public ZoomType ImageZoomType { get; set; }

		public string BackgroundImageAbsolutePath { get; set; }

        public TimeSpan UpdateImageInterval { get; set; }
        public TimeSpan ImageFadeAnimationInterval { get; set; }
		public string BackgroundImagesDirectoryAbsolutePath { get; set; }
        public string Extensions { get; set; }
        public bool LoopSlideshow { get; set; }

        public void Serialize()
		{
			var config = JsonSerializer<Setting>.Serialize(this);

			var assemblylocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			var configpath = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, CONFIGFILE);

			using (var s = new StreamWriter(configpath, false, Encoding.ASCII))
			{
				s.Write(config);
				s.Close();
			}
		}

        public static Setting Initialize(IServiceProvider serviceProvider)
        {
            var settings = Setting.Instance;
            if (settings.ServiceProvider != serviceProvider)
            {
                settings.ServiceProvider = serviceProvider;
            }
            try
            {
                settings.Load();
            }
            catch
            {
                return Setting.Deserialize();
            }
            return settings;
        }

        public void Load()
        {
            var _DTE2 = (DTE2)ServiceProvider.GetService(typeof(DTE));
            var props = _DTE2.Properties["ClaudiaIDE", "General"];

            BackgroundImagesDirectoryAbsolutePath = Setting.ToFullPath((string)props.Item("BackgroundImageDirectoryAbsolutePath").Value);
            BackgroundImageAbsolutePath = Setting.ToFullPath((string)props.Item("BackgroundImageAbsolutePath").Value);
            Opacity = (double)props.Item("Opacity").Value;
            PositionHorizon = (PositionH)props.Item("PositionHorizon").Value;
            PositionVertical = (PositionV)props.Item("PositionVertical").Value;
            UpdateImageInterval = (TimeSpan)props.Item("UpdateImageInterval").Value;
            Extensions = (string)props.Item("Extensions").Value;
            ImageBackgroundType = (ImageBackgroundType)props.Item("ImageBackgroundType").Value;
            ImageFadeAnimationInterval = (TimeSpan)props.Item("ImageFadeAnimationInterval").Value;
            LoopSlideshow = (bool)props.Item("LoopSlideshow").Value;
            MaxWidth = (int)props.Item("MaxWidth").Value;
            MaxHeight = (int)props.Item("MaxHeight").Value;
			ImageZoomType = (ZoomType)props.Item("ImageZoomType").Value;
		}

        public void OnApplyChanged()
        {
            Load();
            OnChange(this);
        }

        public static Setting Deserialize()
		{
			var assemblylocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			var configpath = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, CONFIGFILE);
			string config = "";

			using (var s = new StreamReader(configpath, Encoding.ASCII, false))
			{
				config = s.ReadToEnd();
				s.Close();
			}
			var ret = JsonSerializer<Setting>.DeSerialize(config);
            ret.BackgroundImageAbsolutePath = ToFullPath(ret.BackgroundImageAbsolutePath);
            ret.BackgroundImagesDirectoryAbsolutePath = ToFullPath(ret.BackgroundImagesDirectoryAbsolutePath);
			return ret;
		}

		public static string ToFullPath(string path)
		{
			var assemblylocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			if (!Path.IsPathRooted(path))
			{
				path = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, path);
			}
			return path;
		}

        private void OnChange(object sender)
        {
            if(OnChanged != null)
            {
                OnChanged(sender, EventArgs.Empty);
            }
        }
	}

	[CLSCompliant(false), ComVisible(true)]
	[Guid("12d9a45f-ec0b-4a96-88dc-b0cba1f4789a")]
	public enum PositionV
	{
		Top,
		Bottom,
		Center
	}

	[CLSCompliant(false), ComVisible(true)]
	[Guid("8b2e3ece-fbf7-43ba-b369-3463726b828d")]
	public enum PositionH
	{
		Left,
		Right,
		Center
	}

    [CLSCompliant(false), ComVisible(true)]
    [Guid("5C96CFAA-FE54-49A9-8AB7-E85B66731228")]
    public enum ImageBackgroundType
    {
        Single = 0,
        Slideshow = 1
    }

	[CLSCompliant(false), ComVisible(true)]
	[Guid("67AD0862-C3A2-4280-B0C9-4F9B3306CCF8")]
	public enum ZoomType 
	{
		Zoom = 0,
		Stretch = 1
	}
}
