using System.IO;
using System.Xml.Serialization;

namespace POEStashSorter
{
	public class ConfigParameters
	{
		public string Cookie { get; set; } = "";
		public string League { get; set; } = "";
		public string AccountName { get; set; } = "";
	}
	public static class ConfigManager
	{
		public static ConfigParameters Parameters = new ConfigParameters();
		public static string FileName = @"";

		public static bool Save()
		{
			if (string.IsNullOrWhiteSpace(FileName)) return false;
			TextWriter tw = null;
			try
			{
				Validate();
				if (File.Exists(FileName))
					File.Delete(FileName);
				else
					Directory.CreateDirectory(Path.GetDirectoryName(FileName));
				tw = new StreamWriter(FileName);
				XmlSerializer xs = new XmlSerializer(typeof(ConfigParameters));
				xs.Serialize(tw, Parameters);
				tw.Close();
				return true;
			} catch
			{
				if (tw != null) tw.Close();
				return false;
			}
		}

		public static void Validate()
		{
		}

		public static bool Load()
		{
			if (string.IsNullOrWhiteSpace(FileName)) return false;
			if (File.Exists(FileName))
			{
				StreamReader sr = null;
				try
				{
					sr = new StreamReader(FileName);
					XmlSerializer xs = new XmlSerializer(typeof(ConfigParameters));
					Parameters = (ConfigParameters)xs.Deserialize(sr);
					sr.Close();
					Validate();
					return true;
				} catch
				{
					if (sr != null) sr.Close();
					if (File.Exists(FileName))
						File.Delete(FileName);
					Parameters = new ConfigParameters();
					return false;
				}
			}
			return true;
		}
	}
}
