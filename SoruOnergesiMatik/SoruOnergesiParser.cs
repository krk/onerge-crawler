using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SoruOnergesiMatik
{
	public class SoruOnergesiParser
	{
		private static readonly Regex reOnergeNo =
			new Regex("A HREF=\"/develop/owa/yazili_sozlu_soru_sd.onerge_bilgileri[?]kanunlar_sira_no=(\\d+)\"",
				RegexOptions.Compiled);

		private static readonly Regex reOnergeAllPages =
			new Regex("A HREF=\"(/develop/owa/yazili_sozlu_soru_sd\\.sorgu_sonuc[?]taksim_no=0&kullanici_id=\\d+&sonuc_sira=\\d+&bulunan_kayit=\\d+&metin_arama=&icerik_arama=)\">",
				RegexOptions.Compiled);

		private const string _host = "https://www.tbmm.gov.tr";

		public IEnumerable<string> ParseKanunSiraNo(string content)
		{
			foreach (Match match in reOnergeNo.Matches(content))
			{
				var kanunSiraNo = match.Groups[1].Captures[0].Value;

				yield return kanunSiraNo;
			}
		}

		public IEnumerable<string> GetAllPagerLinks(string content)
		{
			foreach (Match match in reOnergeAllPages.Matches(content))
			{
				var link = match.Groups[1].Captures[0].Value;

				yield return string.Concat(_host, link);
			}
		}

		private static readonly Regex reDonemiVeYasamaYili = new Regex("Dönemi ve Yasama.+</B></TD>\\s*<TD>([^<]+)</TD>", RegexOptions.Compiled);
		private static readonly Regex reEsasNumarasi = new Regex("Esas Numaras.+</B></TD>\\s*<TD>([^<]+)</TD>", RegexOptions.Compiled);
		private static readonly Regex reBaskanligaGelisTarihi = new Regex("Ba.kanl..a Geli. Tarihi.+</B></TD>\\s*<TD>([^<]+)</TD>", RegexOptions.Compiled);
		private static readonly Regex reOnergeninOzeti = new Regex("Önergenin Özeti.+</B></TD>\\s*<TD>([^<]+)</TD>", RegexOptions.Compiled);
		private static readonly Regex reOnergeninSahibi = new Regex("Önergenin Sahibi.+</B></TD>\\s*<TD>.*?\\d+\">([^<]+)", RegexOptions.Compiled);
		private static readonly Regex reOnergeninMuhatabi = new Regex("Önergenin Muhatab.+</B></TD>\\s*<TD>.*?\\d+\">([^<]+)", RegexOptions.Compiled);
		private static readonly Regex reOnergeyiCevaplayan = new Regex("Önergeyi Cevaplayan.+</B></TD>\\s*<TD>.*?\\d*\">([^<]+)", RegexOptions.Compiled);
		private static readonly Regex reOnergeninSonDurumu = new Regex("Önergenin Son Durumu.+</B></TD>\\s*<TD>([^<]+)</TD>", RegexOptions.Compiled);
		private static readonly Regex reOnergeMetniLink = new Regex("<A HREF=\"(http://www2.tbmm.gov.tr/[^\"]+)\">[^<]+Soru Önergesinin Metni", RegexOptions.Compiled);

		public OnergeDetay ParseOnergeDetay(string content)
		{
			var ret = new OnergeDetay();

			Action<Regex, Action<OnergeDetay, string>> findAndSet = (regex, setter) =>
			{
				var match = regex.Match(content);
				var value = match.Groups[1].Captures[0].Value;
				value = ReplaceTurkishCharacters(value);
				value = GraphCommonsBugWorkaroundSanitizer(value);

				setter(ret, value);
			};

			findAndSet(reDonemiVeYasamaYili, (detay, _) => detay.DonemiVeYasamaYili = _);
			findAndSet(reEsasNumarasi, (detay, _) => detay.EsasNumarasi = _);
			findAndSet(reBaskanligaGelisTarihi, (detay, _) => detay.BaskanligaGelisTarigi = _);
			findAndSet(reOnergeninOzeti, (detay, _) => detay.OnergeninOzeti = _);
			findAndSet(reOnergeninSahibi, (detay, _) => detay.OnergeninSahibi = _);
			findAndSet(reOnergeninMuhatabi, (detay, _) => detay.OnergeninMuhatabi = _);
			findAndSet(reOnergeyiCevaplayan, (detay, _) => detay.OnergeyiCevaplayan = _);
			findAndSet(reOnergeninSonDurumu, (detay, _) => detay.OnergeninSonDurumu = _);
			findAndSet(reOnergeMetniLink, (detay, _) => detay.OnergeMetniLink = _);

			// <A HREF="(http://www2.tbmm.gov.tr/[^"]+)">[^<]+Soru Önergesinin Metni
			ret.Parti = ret.OnergeninSahibi.Split(' ').FirstOrDefault();

			return ret;
		}

		private static string GraphCommonsBugWorkaroundSanitizer(string text)
		{
			// comma cannot be escaped by quoting.
			return text.Replace(",", "");
		}

		private static string ReplaceTurkishCharacters(string text)
		{
			var ret = new StringBuilder(text);

			string from = "ðüþiöçÐÜÞÝÖÇý";
			string to = "ğüşiöçĞÜŞİÖÇı";

			for (int i = 0; i < from.Length; i++)
			{
				ret.Replace(from[i], to[i]);
			}

			return ret.ToString();
		}

		private static readonly Regex reSicilNo = new Regex("<OPTION value=\"(\\d+)\">", RegexOptions.Compiled);

		public IEnumerable<string> ParseMVSicil(string content)
		{
			foreach (Match match in reSicilNo.Matches(content))
			{
				var sicilNo = match.Groups[1].Captures[0].Value;

				yield return sicilNo;
			}
		}
	}
}