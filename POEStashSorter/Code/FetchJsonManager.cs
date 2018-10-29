using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace POEStashSorter
{
	public class FetchJsonManager
	{
		private string accountName = "";
		private string league = "";
		private string url = @"https://pathofexile.com/character-window/get-stash-items?accountName=[accountName]&tabIndex=[tabIndex]&league=[league]&tabs=1";

		private string cookie = "";

		public FetchJsonManager(string Cookie, string AccountName, string League = "Delve")
		{
			accountName = AccountName;
			league = League;
			cookie = Cookie;
		}

		public string FetchStashTabJsonString(string tabIndex)
		{
			using (WebClient client = new WebClient())
			{
				string urlWithParams = url.Replace("[accountName]", accountName).Replace("[league]", league).Replace("[tabIndex]", tabIndex);
				client.Headers.Add(HttpRequestHeader.Accept, @"application/json, text/javascript, */*; q=0.01");
				//client.Headers.Add(HttpRequestHeader.AcceptEncoding, @"gzip, deflate, br");
				client.Headers.Add(HttpRequestHeader.AcceptLanguage, @"ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
				//client.Headers.Add(HttpRequestHeader.Connection, @"keep-alive");
				client.Headers.Add(HttpRequestHeader.Cookie, cookie);
				client.Headers.Add(HttpRequestHeader.Host, @"pathofexile.com");
				client.Headers.Add(HttpRequestHeader.Referer, @"https://pathofexile.com/account/view-profile/" + accountName);  // CHECK
				client.Headers.Add(HttpRequestHeader.UserAgent, @"Mozilla/5.0 (Windows NT 10.0; …) Gecko/20100101 Firefox/62.0");
				return client.DownloadString(urlWithParams);
			}
		}

		public JsonResponse FetchStashTabJSON(string tabIndex)
		{
			return JsonConvert.DeserializeObject<JsonResponse>(FetchStashTabJsonString(tabIndex));
		}
	}
}
