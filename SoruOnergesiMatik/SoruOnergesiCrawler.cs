using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SoruOnergesiMatik
{
	public class SoruOnergesiCrawler
	{
		private readonly string _cacheDir;

		private string GetContentFromCache(string url)
		{
			var hash = CalculateMD5Hash(url);

			var file = Path.Combine(_cacheDir, hash);

			if (File.Exists(file))
			{
				return File.ReadAllText(file);
			}

			return null;
		}

		private void SetCacheContent(string url, string content)
		{
			var hash = CalculateMD5Hash(url);

			var file = Path.Combine(_cacheDir, hash);

			File.WriteAllText(file, content);
		}

		private static string CalculateMD5Hash(string input)
		{
			MD5 md5 = MD5.Create();
			byte[] inputBytes = Encoding.ASCII.GetBytes(input);
			byte[] hash = md5.ComputeHash(inputBytes);

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}
			return sb.ToString();

		}

		public SoruOnergesiCrawler(string cacheDir)
		{
			_cacheDir = cacheDir;
			Directory.CreateDirectory(cacheDir);
		}

		public string GetFirstPage(string sicilNo)
		{
			const string baslangic = "https://www.tbmm.gov.tr/develop/owa/yazili_sozlu_soru_sd.sorgu_yonlendirme";

			var cacheKey = string.Concat(baslangic, "+", sicilNo);

			var cacheContent = GetContentFromCache(cacheKey);

			if (cacheContent != null)
			{
				return cacheContent;
			}

			// om_sicil 6974

			using (var wc = new WebClient())
			{
				var values = new NameValueCollection();

				values["d_yy"] = "999";
				values["taksim_no"] = "0";
				values["os_sicil"] = "0";
				values["om_sicil"] = sicilNo;
				values["y_d_d_k"] = "0";
				values["esas_no"] = "";
				values["genel_evrak_tarihi_basla"] = "";
				values["genel_evrak_tarihi_bitis"] = "";
				values["metin_arama"] = "";
				values["icerik_arama"] = "";

				// d_yy=999&taksim_no=0&os_sicil=0&om_sicil=0&y_d_d_k=0
				// &esas_no=&genel_evrak_tarihi_basla=&genel_evrak_tarihi_bitis=&metin_arama=&icerik_arama=

				wc.Headers["Content-Type"] = "application/x-www-form-urlencoded";

				var response = wc.UploadValues(baslangic, "POST", values);
				var text = Encoding.UTF8.GetString(response);

				SetCacheContent(cacheKey, text);

				return text;
			}
		}

		public string GetSicilNosPage()
		{
			return GetPage("https://www.tbmm.gov.tr/develop/owa/yazili_sozlu_soru_sd.sorgu_baslangic");
		}

		public string GetPage(string link)
		{
			var cacheContent = GetContentFromCache(link);

			if (cacheContent != null)
			{
				return cacheContent;
			}

			using (var wc = new WebClient())
			{
				var content = wc.DownloadString(link);

				SetCacheContent(link, content);

				return content;
			}
		}

		public static string GetOnergeDetayLink(string kanunSiraNo)
		{
			return $"https://www.tbmm.gov.tr/develop/owa/yazili_sozlu_soru_sd.onerge_bilgileri?kanunlar_sira_no={kanunSiraNo}";
		}
	}
}