using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoruOnergesiMatik
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("No guarantee whatsoever, provided as is.");

			var crawler = new SoruOnergesiCrawler("cache");

			var parser = new SoruOnergesiParser();

			var sicilNosPage = crawler.GetSicilNosPage();

			var sicilNos = parser.ParseMVSicil(sicilNosPage).Where(_ => _ != "0").Distinct().OrderBy(_ => _);

			var onergeDetays = new List<OnergeDetay>();

			foreach (var sicilNo in sicilNos)
			{
				// sicilNo may or may not be a valid sicil no.
				Console.WriteLine($"Sicil No: {sicilNo}");

				var firstPage = crawler.GetFirstPage(sicilNo);

				var kanunSiraNos = new List<string>();

				kanunSiraNos.AddRange(parser.ParseKanunSiraNo(firstPage));

				foreach (var pageLink in parser.GetAllPagerLinks(firstPage).Skip(1))
				{
					var content = crawler.GetPage(pageLink);

					kanunSiraNos.AddRange(parser.ParseKanunSiraNo(content));
				}

				var uniqKanunSiraNos = kanunSiraNos.Distinct().OrderBy(_ => _).ToList();

				foreach (var kanunSiraNo in uniqKanunSiraNos)
				{
					var content = crawler.GetPage(SoruOnergesiCrawler.GetOnergeDetayLink(kanunSiraNo));

					var onergeDetay = parser.ParseOnergeDetay(content);

					onergeDetays.Add(onergeDetay);

					Console.WriteLine($"Parsed {kanunSiraNo}");
				}
			}

			var dotConverter = new DotConverter();

			var dot = dotConverter.GetDot(onergeDetays);

			var gcConverter = new GraphCommonsCsvConverter();

			var csvs = gcConverter.GetEdgesCsv(onergeDetays).ToList();
			var nodesCsv = gcConverter.GetNodesCsv(onergeDetays);

			const string outputDir = "output";

			Directory.CreateDirectory(outputDir);

			File.WriteAllText(Path.Combine(outputDir, "onerge.dot"), dot);
			File.WriteAllText(Path.Combine(outputDir, "nodes.csv"), nodesCsv);

			int i = 1;

			foreach (var csv in csvs)
			{
				File.WriteAllText(Path.Combine(outputDir, $"edges{i}.csv"), csv);

				i++;
			}
		}
	}
}