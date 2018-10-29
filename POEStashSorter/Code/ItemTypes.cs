using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace POEStashSorter
{
	public static class ItemBaseTypes
	{
		public static List<string> Maps = new List<string>();
		public static List<string> Essences = new List<string>();
		public static List<string> Divination = new List<string>();

		public static string GetItemBaseType(string s)
		{
			var r = Maps.Find(x => s.Contains(x)) ?? 
				Essences.Find(x => s.Contains(x)) ?? 
				Divination.Find(x => s.Contains(x)) ?? 
				s;
			return r;
		}

		static ItemBaseTypes()
		{
			var f = File.OpenText(@"Data\ItemBaseTypes.json");
			dynamic d = JObject.Parse(f.ReadToEnd());
			f.Close();
			Maps.AddRange((d["Map"]["types"] as JArray).Select(x => (string)x).ToArray());
			Essences.AddRange((d["EssenceNormalized"]["types"] as JArray).Select(x => (string)x).ToArray());
			Divination.AddRange((d["Divination-card"]["types"] as JArray).Select(x => (string)x).ToArray());
			
		}
	}
}