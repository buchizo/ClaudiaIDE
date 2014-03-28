using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.Shell;

namespace ClaudiaIDE.Settings
{
	public class Setting
	{
		private static readonly string CONFIGFILE = "config.txt";

		public Setting()
		{
			var assemblylocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			BackgroundImageAbsolutePath = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation, "background.png");
			Opacity = 0.35;
			PositionHorizon = PositionH.Right;
			PositionVertical = PositionV.Bottom;
		}

		public double Opacity { get; set; }
		public string BackgroundImageAbsolutePath { get; set; }
		public PositionV PositionVertical { get; set; }
		public PositionH PositionHorizon { get; set; }

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

		public static Setting Desirialize()
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
}
