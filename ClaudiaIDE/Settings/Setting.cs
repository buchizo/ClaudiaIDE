using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace ClaudiaIDE.Settings
{
    public class Setting
    {
        private static readonly Setting instance = new Setting();
        private static readonly string CONFIGFILE = "config.txt";
        private const string DefaultBackgroundImage = "Images\\background.png";
        private const string DefaultBackgroundFolder = "Images";

        internal System.IServiceProvider ServiceProvider { get; set; }

        public WeakEvent<EventArgs> OnChanged = new WeakEvent<EventArgs>();

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
            BackgroundImagesDirectoryAbsolutePath = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, DefaultBackgroundFolder);
            BackgroundImageAbsolutePath = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, DefaultBackgroundImage);
            Opacity = 0.35;
            PositionHorizon = PositionH.Right;
            PositionVertical = PositionV.Bottom;
            ImageStretch = ImageStretch.None;
            UpdateImageInterval = TimeSpan.FromMinutes(30);
            ImageFadeAnimationInterval = TimeSpan.FromSeconds(0);
            Extensions = ".png, .jpg";
            ImageBackgroundType = ImageBackgroundType.Single;
            LoopSlideshow = true;
            ShuffleSlideshow = false;
            MaxWidth = 0;
            MaxHeight = 0;
            SoftEdgeX = 0;
            SoftEdgeY = 0;
            ExpandToIDE = false;
            ViewBoxPointX = 0;
            ViewBoxPointY = 0;
        }

        public ImageBackgroundType ImageBackgroundType { get; set; }
        public double Opacity { get; set; }
        public PositionV PositionVertical { get; set; }
        public PositionH PositionHorizon { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
        public int SoftEdgeX { get; set; }
        public int SoftEdgeY { get; set; }
        
        public string BackgroundImageAbsolutePath { get; set; }

        public TimeSpan UpdateImageInterval { get; set; }
        public TimeSpan ImageFadeAnimationInterval { get; set; }
        public string BackgroundImagesDirectoryAbsolutePath { get; set; }
        public string Extensions { get; set; }
        public bool LoopSlideshow { get; set; }
        public bool ShuffleSlideshow { get; set; }
        public ImageStretch ImageStretch { get; set; }
        public bool ExpandToIDE { get; set; }
        public double ViewBoxPointX { get; set; }
        public double ViewBoxPointY { get; set; }

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

        public static async Task<Setting> InitializeAsync(IServiceProvider serviceProvider)
        {
            var settings = Setting.Instance;
            if (settings.ServiceProvider != serviceProvider)
            {
                settings.ServiceProvider = serviceProvider;
            }
            try
            {
                await settings.LoadAsync();
            }
            catch
            {
                return Setting.Deserialize();
            }
            return settings;
        }

        public async Task LoadAsync()
        {
            Microsoft.VisualStudio.Shell.IAsyncServiceProvider asyncServiceProvider = await ((Microsoft.VisualStudio.Shell.AsyncPackage)ServiceProvider).GetServiceAsync(typeof(Microsoft.VisualStudio.Shell.Interop.SAsyncServiceProvider)) as Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
            var testService = await asyncServiceProvider.GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            var props = testService.Properties["ClaudiaIDE", "General"];

            Load(props);
        }

        private void Load(Properties props)
        {
            BackgroundImagesDirectoryAbsolutePath = Setting.ToFullPath((string)props.Item("BackgroundImageDirectoryAbsolutePath").Value, DefaultBackgroundFolder);
            BackgroundImageAbsolutePath = Setting.ToFullPath((string)props.Item("BackgroundImageAbsolutePath").Value, DefaultBackgroundImage);
            Opacity = (double)props.Item("Opacity").Value;
            PositionHorizon = (PositionH)props.Item("PositionHorizon").Value;
            PositionVertical = (PositionV)props.Item("PositionVertical").Value;
            ImageStretch = (ImageStretch)props.Item("ImageStretch").Value;
            UpdateImageInterval = (TimeSpan)props.Item("UpdateImageInterval").Value;
            Extensions = (string)props.Item("Extensions").Value;
            ImageBackgroundType = (ImageBackgroundType)props.Item("ImageBackgroundType").Value;
            LoopSlideshow = (bool)props.Item("LoopSlideshow").Value;
            ShuffleSlideshow = (bool)props.Item("ShuffleSlideshow").Value;
            MaxWidth = (int)props.Item("MaxWidth").Value;
            MaxHeight = (int)props.Item("MaxHeight").Value;
            SoftEdgeX = (int)props.Item("SoftEdgeX").Value;
            SoftEdgeY = (int)props.Item("SoftEdgeY").Value;
            ExpandToIDE = (bool)props.Item("ExpandToIDE").Value;
            ViewBoxPointX = (double)props.Item("ViewBoxPointX").Value;
            ViewBoxPointY = (double)props.Item("ViewBoxPointY").Value;
        }

        public void Load()
        {
            var _DTE2 = (DTE2)ServiceProvider.GetService(typeof(DTE));
            var props = _DTE2.Properties["ClaudiaIDE", "General"];

            Load(props);
        }

        public void OnApplyChanged()
        {
            try
            {
                Load();
                OnChanged?.RaiseEvent(this, EventArgs.Empty);
            }
            catch
            {

            }
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
            ret.BackgroundImageAbsolutePath = ToFullPath(ret.BackgroundImageAbsolutePath, DefaultBackgroundImage);
            ret.BackgroundImagesDirectoryAbsolutePath = ToFullPath(ret.BackgroundImagesDirectoryAbsolutePath, DefaultBackgroundFolder);
            return ret;
        }

        public static string ToFullPath(string path, string defaultPath)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = defaultPath;
            }
            var assemblylocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, path);
            }
            return path;
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
        Slideshow = 1,
        SingleEach = 2
    }

    [CLSCompliant(false), ComVisible(true)]
    [Guid("C89AFB79-39AF-4716-BB91-0F77323DD89B")]
    public enum ImageStretch
    {
        None = 0,
        Uniform = 1,
        UniformToFill = 2,
        Fill = 3
    }

    public static class ImageStretchConverter
    {
        public static System.Windows.Media.Stretch ConvertTo(this ImageStretch source)
        {
            switch(source)
            {
                case ImageStretch.Fill:
                    return System.Windows.Media.Stretch.Fill;
                case ImageStretch.None:
                    return System.Windows.Media.Stretch.None;
                case ImageStretch.Uniform:
                    return System.Windows.Media.Stretch.Uniform;
                case ImageStretch.UniformToFill:
                    return System.Windows.Media.Stretch.UniformToFill;
            }
            return System.Windows.Media.Stretch.None;
        }
    }

    public static class PositionConverter
    {
        public static System.Windows.Media.AlignmentY ConvertTo(this PositionV source)
        {
            switch (source)
            {
                case PositionV.Bottom:
                    return System.Windows.Media.AlignmentY.Bottom;
                case PositionV.Center:
                    return System.Windows.Media.AlignmentY.Center;
                case PositionV.Top:
                    return System.Windows.Media.AlignmentY.Top;
            }
            return System.Windows.Media.AlignmentY.Bottom;
        }

        public static System.Windows.VerticalAlignment ConvertToVerticalAlignment(this PositionV source)
        {
            switch (source)
            {
                case PositionV.Bottom:
                    return System.Windows.VerticalAlignment.Bottom;
                case PositionV.Center:
                    return System.Windows.VerticalAlignment.Center;
                case PositionV.Top:
                    return System.Windows.VerticalAlignment.Top;
            }
            return System.Windows.VerticalAlignment.Bottom;
        }

        public static System.Windows.Media.AlignmentX ConvertTo(this PositionH source)
        {
            switch (source)
            {
                case PositionH.Left:
                    return System.Windows.Media.AlignmentX.Left;
                case PositionH.Center:
                    return System.Windows.Media.AlignmentX.Center;
                case PositionH.Right:
                    return System.Windows.Media.AlignmentX.Right;
            }
            return System.Windows.Media.AlignmentX.Right;
        }

        public static System.Windows.HorizontalAlignment ConvertToHorizontalAlignment(this PositionH source)
        {
            switch (source)
            {
                case PositionH.Left:
                    return System.Windows.HorizontalAlignment.Left;
                case PositionH.Center:
                    return System.Windows.HorizontalAlignment.Center;
                case PositionH.Right:
                    return System.Windows.HorizontalAlignment.Right;
            }
            return System.Windows.HorizontalAlignment.Right;
        }
    }
}
