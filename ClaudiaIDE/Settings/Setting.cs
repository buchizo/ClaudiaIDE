using System.IO;
using System.Text;

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
			if (!Path.IsPathRooted(ret.BackgroundImageAbsolutePath))
			{
				ret.BackgroundImageAbsolutePath = Path.Combine(string.IsNullOrEmpty(assemblylocation) ? "" : assemblylocation,
				                                               ret.BackgroundImageAbsolutePath);
			}
			return ret;
		}
	}

	public enum PositionV
	{
		Top,
		Bottom
	}

	public enum PositionH
	{
		Left,
		Right
	}
}
