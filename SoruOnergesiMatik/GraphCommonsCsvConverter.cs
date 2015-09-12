using System.Collections.Generic;
using System.Text;

namespace SoruOnergesiMatik
{
	public class GraphCommonsCsvConverter
	{
		public IEnumerable<string> GetEdgesCsv(IEnumerable<OnergeDetay> onergeDetays)
		{
			// SOURCE_NODE_TYPE, SOURCE_NODE_NAME, EDGE_TYPE, TARGET_NODE_TYPE, TARGET_NODE_NAME, WEIGHT
			var quote = "\"";

			var nodes = new HashSet<string>();
			int batchCount = 1;

			var ret = new StringBuilder();

			foreach (var detay in onergeDetays)
			{
				if (nodes.Count + 3 >= batchCount * 500)
				{
					yield return ret.ToString();

					batchCount++;
					ret.Clear();
				}

				ret.AppendFormat("{0}, {1}, {2}, {3}, {4}, {5}\n", "Milletvekili", detay.OnergeninSahibi, "UYE", "Parti", detay.Parti, 1);
				ret.AppendFormat("{0}, {1}, {2}, {3}, {4}, {5}\n", "Milletvekili", detay.OnergeninSahibi, "SAHIP", "Soru Önergesi", detay.EsasNumarasi, 1);
				ret.AppendFormat("{0}, {1}, {2}, {3}, {4}, {5}\n", "Soru Önergesi", quote + detay.EsasNumarasi + quote, "MUHATAP", "Bakan", quote + detay.OnergeninMuhatabi + quote, 1);

				nodes.Add(detay.OnergeninSahibi);
				nodes.Add(detay.Parti);
				nodes.Add(detay.EsasNumarasi);
				nodes.Add(detay.OnergeninMuhatabi);
			}

			yield return ret.ToString();
		}

		public string GetNodesCsv(IEnumerable<OnergeDetay> onergeDetays)
		{
			var quote = "\"";

			// Type	Name	Description	Image	Reference
			var ret = new StringBuilder();

			foreach (var detay in onergeDetays)
			{
				ret.AppendFormat("{0}, {1}, {2}, {3}, {4}\n", "Soru Önergesi", quote + detay.EsasNumarasi + quote, "", "", detay.OnergeMetniLink);
			}

			return ret.ToString();
		}
	}
}