using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using EnvDTE;
using EnvDTE80;
using MetroRadiance.Platform;
using Microsoft.VisualStudio.Shell;

namespace ClaudiaIDE.Settings
{
    public class Setting
    {
        private const string DefaultBackgroundImage = "Images\\background.png";
        private const string DefaultBackgroundFolder = "Images";
        private static readonly List<Setting> _instance = new List<Setting>();
        private static readonly string CONFIGFILE = "config.txt";

        public static string[] SupportVideoFileExtensions = new []{ ".gif", ".mp4" };

        [IgnoreDataMember] public WeakEvent<EventArgs> OnChanged = new WeakEvent<EventArgs>();


        public Setting()
        {
            var assemblylocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            BackgroundImagesDirectoryAbsolutePath =
                Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, DefaultBackgroundFolder);
            BackgroundImageAbsolutePath = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation,
                DefaultBackgroundImage);
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
            BlurRadius = 0;
            BlurMethod = ImageBlurMethod.Gaussian;
            ExpandToIDE = false;
            ViewBoxPointX = 0;
            ViewBoxPointY = 0;
            IsLimitToMainlyEditorWindow = false;
            ViewPortHeight = 1;
            ViewPortWidth = 1;
            ViewPortPointX = 0;
            ViewPortPointY = 0;
            TileMode = TileMode.None;
            WebSingleUrl = "";
            WebApiEndpoint = "";
            WebApiJsonKey = "";
            WebApiDownloadInterval = TimeSpan.FromMinutes(5);
            IsTransparentToContentMargin = false;
            IsTransparentToStickyScroll = false;
        }

        internal DTE ServiceProvider { get; set; }

        [IgnoreDataMember] public string SolutionConfigFilePath { get; set; }

        public static Setting Instance
        {
            get
            {
                var solfile = VisualStudioUtility.GetSolutionSettingsFileFullPath();
                var i = _instance?.FirstOrDefault(x => x.SolutionConfigFilePath == solfile);
                if (i == null)
                {
                    i = new Setting
                    {
                        SolutionConfigFilePath = solfile
                    };
                    _instance.Add(i);
                }

                return i;
            }
        }

        public static Setting DefaultInstance
        {
            get
            {
                var i = _instance?.FirstOrDefault(x => x.SolutionConfigFilePath == null);
                if (i == null)
                {
                    i = new Setting();
                    _instance.Add(i);
                }

                return i;
            }
        }

        public ImageBackgroundType ImageBackgroundType { get; set; }
        public double Opacity { get; set; }
        public PositionV PositionVertical { get; set; }
        public PositionH PositionHorizon { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
        public int SoftEdgeX { get; set; }
        public int SoftEdgeY { get; set; }
        public int BlurRadius { get; set; }
        public ImageBlurMethod BlurMethod { get; set; }

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
        public bool IsLimitToMainlyEditorWindow { get; set; }
        public double ViewPortWidth { get; set; }
        public double ViewPortHeight { get; set; }
        public double ViewPortPointX { get; set; }
        public double ViewPortPointY { get; set; }
        public TileMode TileMode { get; set; }
        public string WebSingleUrl { get; set; }
        public string WebApiEndpoint { get; set; }
        public string WebApiJsonKey { get; set; }
        public TimeSpan WebApiDownloadInterval { get; set; }
        public bool IsTransparentToContentMargin { get; set; }
        public bool IsTransparentToStickyScroll { get; set; }

        [IgnoreDataMember]
        public bool IsHidden { get; set; }

        public void Serialize()
        {
            var config = JsonSerializer<Setting>.Serialize(this);

            var assemblylocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configpath = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, CONFIGFILE);

            using (var s = new StreamWriter(configpath, false, Encoding.UTF8))
            {
                s.Write(config);
                s.Close();
            }
        }

        public void Serialize(string solutionConfigPath)
        {
            var config = JsonSerializer<Setting>.Serialize(this);

            using (var s = new StreamWriter(solutionConfigPath, false, Encoding.UTF8))
            {
                s.Write(config);
                s.Close();
            }
        }

        public static Setting Initialize(DTE serviceProvider)
        {
            var settings = Instance;
            if (settings.ServiceProvider == null || settings.ServiceProvider != serviceProvider)
                settings.ServiceProvider = serviceProvider;
            try
            {
                var solfile = VisualStudioUtility.GetSolutionSettingsFileFullPath();
                if (!string.IsNullOrWhiteSpace(solfile))
                {
                    var solconf = Deserialize(solfile);
                    settings.BackgroundImageAbsolutePath = solconf.BackgroundImageAbsolutePath;
                    settings.BackgroundImagesDirectoryAbsolutePath = solconf.BackgroundImagesDirectoryAbsolutePath;
                    settings.ExpandToIDE = solconf.ExpandToIDE;
                    settings.Extensions = solconf.Extensions;
                    settings.ImageBackgroundType = solconf.ImageBackgroundType;
                    settings.ImageFadeAnimationInterval = solconf.ImageFadeAnimationInterval;
                    settings.ImageStretch = solconf.ImageStretch;
                    settings.LoopSlideshow = solconf.LoopSlideshow;
                    settings.MaxHeight = solconf.MaxHeight;
                    settings.MaxWidth = solconf.MaxWidth;
                    settings.Opacity = solconf.Opacity;
                    settings.PositionHorizon = solconf.PositionHorizon;
                    settings.PositionVertical = solconf.PositionVertical;
                    settings.ShuffleSlideshow = solconf.ShuffleSlideshow;
                    settings.SoftEdgeX = solconf.SoftEdgeX;
                    settings.SoftEdgeY = solconf.SoftEdgeY;
                    settings.BlurRadius = solconf.BlurRadius;
                    settings.BlurMethod = solconf.BlurMethod;
                    settings.UpdateImageInterval = solconf.UpdateImageInterval;
                    settings.ViewBoxPointX = solconf.ViewBoxPointX;
                    settings.ViewBoxPointY = solconf.ViewBoxPointY;
                    settings.SolutionConfigFilePath = solfile;
                    settings.IsLimitToMainlyEditorWindow = solconf.IsLimitToMainlyEditorWindow;
                    settings.TileMode = solconf.TileMode;
                    settings.ViewPortHeight = solconf.ViewPortHeight;
                    settings.ViewPortWidth = solconf.ViewPortWidth;
                    settings.ViewPortPointX = solconf.ViewPortPointX;
                    settings.ViewPortPointY = solconf.ViewPortPointY;
                    settings.WebSingleUrl = solconf.WebSingleUrl;
                    settings.WebApiEndpoint = solconf.WebApiEndpoint;
                    settings.WebApiJsonKey = solconf.WebApiJsonKey;
                    settings.WebApiDownloadInterval = solconf.WebApiDownloadInterval;
                    settings.IsTransparentToStickyScroll = solconf.IsTransparentToStickyScroll;
                    settings.IsTransparentToContentMargin = solconf.IsTransparentToContentMargin;
                }
                else
                {
                    settings.Load();
                }
            }
            catch
            {
                return Deserialize();
            }

            return settings;
        }

        private void Load(Properties props)
        {
            if (props == null) return;
            ThreadHelper.ThrowIfNotOnUIThread();
            BackgroundImagesDirectoryAbsolutePath =
                ToFullPath((string)props.Item("BackgroundImageDirectoryAbsolutePath").Value, DefaultBackgroundFolder);
            BackgroundImageAbsolutePath = ToFullPath((string)props.Item("BackgroundImageAbsolutePath").Value,
                DefaultBackgroundImage);
            Opacity = (double)props.Item("Opacity").Value;
            PositionHorizon = (PositionH)props.Item("PositionHorizon").Value;
            PositionVertical = (PositionV)props.Item("PositionVertical").Value;
            ImageStretch = (ImageStretch)props.Item("ImageStretch").Value;
            UpdateImageInterval = (TimeSpan)props.Item("UpdateImageInterval").Value;
            Extensions = (string)props.Item("Extensions").Value;
            ImageBackgroundType = (ImageBackgroundType)props.Item("ImageBackgroundType").Value;
            ImageFadeAnimationInterval = (TimeSpan)props.Item("ImageFadeAnimationInterval").Value;
            LoopSlideshow = (bool)props.Item("LoopSlideshow").Value;
            ShuffleSlideshow = (bool)props.Item("ShuffleSlideshow").Value;
            MaxWidth = (int)props.Item("MaxWidth").Value;
            MaxHeight = (int)props.Item("MaxHeight").Value;
            SoftEdgeX = (int)props.Item("SoftEdgeX").Value;
            SoftEdgeY = (int)props.Item("SoftEdgeY").Value;
            BlurRadius = (int)props.Item("BlurRadius").Value;
            BlurMethod = (ImageBlurMethod)props.Item("BlurMethod").Value;
            ExpandToIDE = (bool)props.Item("ExpandToIDE").Value;
            ViewBoxPointX = (double)props.Item("ViewBoxPointX").Value;
            ViewBoxPointY = (double)props.Item("ViewBoxPointY").Value;
            IsLimitToMainlyEditorWindow = (bool)props.Item("IsLimitToMainlyEditorWindow").Value;
            TileMode = (TileMode)props.Item("TileMode").Value;
            ViewPortHeight = (double)props.Item("ViewPortHeight").Value;
            ViewPortWidth = (double)props.Item("ViewPortWidth").Value;
            ViewPortPointX = (double)props.Item("ViewPortPointX").Value;
            ViewPortPointY = (double)props.Item("ViewPortPointY").Value;
            WebSingleUrl = (string)props.Item("SingleWebUrl").Value;
            WebApiEndpoint = (string)props.Item("WebApiApiEndpoint").Value;
            WebApiJsonKey = (string)props.Item("WebApiJsonKey").Value;
            WebApiDownloadInterval = (TimeSpan)props.Item("WebApiDownloadInterval").Value;
            IsTransparentToContentMargin = (bool)props.Item("IsTransparentToContentMargin").Value;
            IsTransparentToStickyScroll = (bool)props.Item("IsTransparentToStickyScroll").Value;
        }

        public void Load()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var _DTE2 = (DTE2)ServiceProvider;
            var props = _DTE2?.Properties["ClaudiaIDE", "General"];

            var useColorTheme = (UseColorThemeType)props?.Item("UseColorTheme")?.Value;
            var optionPage = "Light theme";
            switch (useColorTheme)
            {
                case UseColorThemeType.Dark:
                    optionPage = "Dark theme";
                    break;
                case UseColorThemeType.UseSystemSetting:
                    if (MetroRadiance.Platform.WindowsTheme.Theme.Current == Theme.Dark) optionPage = "Dark theme";
                    break;
            }
            Load(_DTE2?.Properties["ClaudiaIDE", optionPage]);
        }

        public void Load(string solutionConfigFile)
        {
            if (string.IsNullOrEmpty(solutionConfigFile))
            {
                Load();
                return;
            }

            var solconf = Deserialize(solutionConfigFile);
            BackgroundImageAbsolutePath = solconf.BackgroundImageAbsolutePath;
            BackgroundImagesDirectoryAbsolutePath = solconf.BackgroundImagesDirectoryAbsolutePath;
            ExpandToIDE = solconf.ExpandToIDE;
            Extensions = solconf.Extensions;
            ImageBackgroundType = solconf.ImageBackgroundType;
            ImageFadeAnimationInterval = solconf.ImageFadeAnimationInterval;
            ImageStretch = solconf.ImageStretch;
            LoopSlideshow = solconf.LoopSlideshow;
            MaxHeight = solconf.MaxHeight;
            MaxWidth = solconf.MaxWidth;
            Opacity = solconf.Opacity;
            PositionHorizon = solconf.PositionHorizon;
            PositionVertical = solconf.PositionVertical;
            ShuffleSlideshow = solconf.ShuffleSlideshow;
            SoftEdgeX = solconf.SoftEdgeX;
            SoftEdgeY = solconf.SoftEdgeY;
            BlurRadius = solconf.BlurRadius;
            BlurMethod = solconf.BlurMethod;
            UpdateImageInterval = solconf.UpdateImageInterval;
            ViewBoxPointX = solconf.ViewBoxPointX;
            ViewBoxPointY = solconf.ViewBoxPointY;
            IsLimitToMainlyEditorWindow = solconf.IsLimitToMainlyEditorWindow;
            TileMode = solconf.TileMode;
            ViewPortHeight = solconf.ViewPortHeight;
            ViewPortWidth = solconf.ViewPortWidth;
            ViewPortHeight = solconf.ViewPortPointX;
            ViewPortWidth = solconf.ViewPortPointY;
            WebSingleUrl = solconf.WebSingleUrl;
            WebApiEndpoint = solconf.WebApiEndpoint;
            WebApiJsonKey = solconf.WebApiJsonKey;
            WebApiDownloadInterval = solconf.WebApiDownloadInterval;
            IsTransparentToStickyScroll = solconf.IsTransparentToStickyScroll;
            IsTransparentToContentMargin = solconf.IsTransparentToContentMargin;
        }

        public void OnApplyChanged()
        {
            try
            {
                Load(SolutionConfigFilePath);
                this.IsHidden = false;
                ProvidersHolder.Instance.ChangeActive(Setting.DefaultInstance.ImageBackgroundType, SolutionConfigFilePath);
                if (string.IsNullOrEmpty(SolutionConfigFilePath))
                {
                    Load();
                    OnChanged?.RaiseEvent(this, EventArgs.Empty);
                }
            }
            catch
            {
            }
        }

        public void OnToggleVisibility()
        {
            try
            {
                this.IsHidden = !this.IsHidden;
                OnChanged?.RaiseEvent(this, EventArgs.Empty);
            }
            catch
            {
            }
        }

        public void OnColorThemeChange()
        {
            try
            {
                Load();
                OnChanged?.RaiseEvent(this, EventArgs.Empty);
            }
            catch { }
        }

        public static Setting Deserialize()
        {
            var assemblylocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configpath = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, CONFIGFILE);
            var config = "";

            using (var s = new StreamReader(configpath, Encoding.UTF8, false))
            {
                config = s.ReadToEnd();
                s.Close();
            }

            var ret = JsonSerializer<Setting>.DeSerialize(config);
            ret.BackgroundImageAbsolutePath = ToFullPath(ret.BackgroundImageAbsolutePath, DefaultBackgroundImage);
            ret.BackgroundImagesDirectoryAbsolutePath =
                ToFullPath(ret.BackgroundImagesDirectoryAbsolutePath, DefaultBackgroundFolder);
            ret.IsHidden = false;
            return ret;
        }

        public static Setting Deserialize(string solutionConfigFilePath)
        {
            var config = "";

            using (var s = new StreamReader(solutionConfigFilePath, Encoding.UTF8, false))
            {
                config = s.ReadToEnd();
                s.Close();
            }

            var ret = JsonSerializer<Setting>.DeSerialize(config);
            ret.BackgroundImageAbsolutePath = ToFullPath(ret.BackgroundImageAbsolutePath, DefaultBackgroundImage);
            ret.BackgroundImagesDirectoryAbsolutePath =
                ToFullPath(ret.BackgroundImagesDirectoryAbsolutePath, DefaultBackgroundFolder);
            ret.IsHidden = false;
            return ret;
        }

        public static string ToFullPath(string path, string defaultPath)
        {
            if (string.IsNullOrWhiteSpace(path)) path = defaultPath;
            var assemblylocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!Path.IsPathRooted(path))
                path = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, path);
            return path;
        }
    }

    [ComVisible(true)]
    [Guid("12d9a45f-ec0b-4a96-88dc-b0cba1f4789a")]
    public enum PositionV
    {
        Top,
        Bottom,
        Center
    }

    [ComVisible(true)]
    [Guid("8b2e3ece-fbf7-43ba-b369-3463726b828d")]
    public enum PositionH
    {
        Left,
        Right,
        Center
    }

    [ComVisible(true)]
    [Guid("5C96CFAA-FE54-49A9-8AB7-E85B66731228")]
    public enum ImageBackgroundType
    {
        Single = 0,
        Slideshow = 1,
        SingleEach = 2,
        WebSingle = 3,
        WebApi = 4
    }

    [ComVisible(true)]
    [Guid("C89AFB79-39AF-4716-BB91-0F77323DD89B")]
    public enum ImageStretch
    {
        None = 0,
        Uniform = 1,
        UniformToFill = 2,
        Fill = 3
    }

    [ComVisible(true)]
    [Guid("0165FD2A-029F-4C3F-A30E-011A19C9E88D")]
    public enum TileMode
    {
        None = 0,
        FlipX = 1,
        FlipY = 2,
        FlipXY = 3,
        Tile = 4
    }

    [ComVisible(true)]
    [Guid("21987719-1230-47DD-B7DD-CF1650784B5A")]
    public enum UseColorThemeType
    {
        Light = 0,
        Dark = 1,
        UseSystemSetting = 2
    }

    [ComVisible(true)]
    [Guid("975050A7-2872-4559-BD55-C589ACB6E2B5")]
    public enum ImageBlurMethod
    {
        Gaussian = 0,
        Box = 1
    }

    public static class ImageStretchConverter
    {
        public static Stretch ConvertTo(this ImageStretch source)
        {
            switch (source)
            {
                case ImageStretch.Fill:
                    return Stretch.Fill;
                case ImageStretch.None:
                    return Stretch.None;
                case ImageStretch.Uniform:
                    return Stretch.Uniform;
                case ImageStretch.UniformToFill:
                    return Stretch.UniformToFill;
            }

            return Stretch.None;
        }
    }

    public static class PositionConverter
    {
        public static AlignmentY ConvertTo(this PositionV source)
        {
            switch (source)
            {
                case PositionV.Bottom:
                    return AlignmentY.Bottom;
                case PositionV.Center:
                    return AlignmentY.Center;
                case PositionV.Top:
                    return AlignmentY.Top;
            }

            return AlignmentY.Bottom;
        }

        public static VerticalAlignment ConvertToVerticalAlignment(this PositionV source)
        {
            switch (source)
            {
                case PositionV.Bottom:
                    return VerticalAlignment.Bottom;
                case PositionV.Center:
                    return VerticalAlignment.Center;
                case PositionV.Top:
                    return VerticalAlignment.Top;
            }

            return VerticalAlignment.Bottom;
        }

        public static AlignmentX ConvertTo(this PositionH source)
        {
            switch (source)
            {
                case PositionH.Left:
                    return AlignmentX.Left;
                case PositionH.Center:
                    return AlignmentX.Center;
                case PositionH.Right:
                    return AlignmentX.Right;
            }

            return AlignmentX.Right;
        }

        public static HorizontalAlignment ConvertToHorizontalAlignment(this PositionH source)
        {
            switch (source)
            {
                case PositionH.Left:
                    return HorizontalAlignment.Left;
                case PositionH.Center:
                    return HorizontalAlignment.Center;
                case PositionH.Right:
                    return HorizontalAlignment.Right;
            }

            return HorizontalAlignment.Right;
        }
    }

    public static class TileModeConverter
    {
        public static System.Windows.Media.TileMode ConvertTo(this TileMode source)
        {
            return (System.Windows.Media.TileMode)source;
        }
    }

    public static class BlurMethodConverter
    {
        public static KernelType ConvertTo(this ImageBlurMethod source)
        {
            return (KernelType)source;
        }
    }
}