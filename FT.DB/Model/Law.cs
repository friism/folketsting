using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FT.DB
{
	public enum LawStage { First, Third, Second };

	public static class LawExtensions
	{
		public static string UrlValue(this LawStage s)
		{
			switch (s)
			{
				case LawStage.First: return 1.ToString();
				case LawStage.Second: return 2.ToString();
				case LawStage.Third: return 3.ToString();
				default: throw new ArgumentException();
			}
		}
	}
}
