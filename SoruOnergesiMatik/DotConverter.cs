using System.Collections.Generic;
using System.Text;

namespace SoruOnergesiMatik
{
	public class DotConverter
	{
		public string GetDot(IEnumerable<OnergeDetay> detaylar)
		{
			var ret = new StringBuilder();
			ret.Append("digraph{");

			foreach (var detay in detaylar)
			{
				ret.AppendFormat("\"{0}\" -> \"{1}\"\n", detay.OnergeninSahibi, detay.EsasNumarasi);
				ret.AppendFormat("\"{0}\" -> \"{1}\"\n", detay.EsasNumarasi, detay.OnergeninMuhatabi);
			}

			ret.Append("}");

			return ret.ToString();
		}
	}
}