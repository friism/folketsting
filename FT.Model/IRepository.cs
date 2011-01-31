using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using FT.DB;

namespace FT.Model
{
	interface IRepository
	{
		DBDataContext DB { get; }
	}

	public abstract class Repository : IRepository
	{
		private readonly DBDataContext _db;

		public Repository()
		{
			_db = new DBDataContext();
		}

		public DBDataContext DB
		{
			get { return _db; }
		}
	}

	public static class Util
	{
		private static readonly Random random = new Random();

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			// Durstenfeld implementation of the Fisher-Yates algorithm for an O(n) unbiased shuffle
			var array = source.ToArray();
			var n = array.Length;
			while (n > 1)
			{
				var k = random.Next(n);
				n--;
				var temp = array[n];
				array[n] = array[k];
				array[k] = temp;
			}

			return array;
		}

		public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> source, int count)
		{
			return source.Shuffle().Take(count);
		}

	}
}
