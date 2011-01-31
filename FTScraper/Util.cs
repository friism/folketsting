using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using System.Net;
//using ScrapeDB;
using FT.DB;
using System.Data.Linq;

namespace FT.Scraper
{
	public static class Util
	{
		public static string ExtractString(byte[] webResult)
		{
			return ExtractString(webResult, null);
		}

		public static string ExtractString(byte[] webResult, Encoding enc)
		{
			//HtmlDocument contract_doc = new HtmlDocument();
			Encoding e = enc ?? Encoding.GetEncoding("ISO-8859-1");
			string s = Encoding.Unicode.GetString(Encoding.Convert(e, Encoding.Unicode, Util.Decompress(webResult)));
			return s;
			//contract_doc.LoadHtml(s);
			//string contractString = contract_doc.DocumentNode.SelectSingleNode("//div[@id=\"fullDocument\"]").OuterHtml;
			//return contractString;
		}

		public static byte[] Decompress(byte[] data)
		{
			Stream s = new GZipInputStream(new MemoryStream(data));

			// I hate this shit.
			int chunkSize = 2048;
			byte[] unzipBytes = new byte[chunkSize];
			int sizeRead;
			MemoryStream ms = new MemoryStream();
			while (true)
			{
				sizeRead = s.Read(unzipBytes, 0, chunkSize);
				if (sizeRead > 0)
					ms.Write(unzipBytes, 0, chunkSize);
				else
					break;
			}

			s.Close();
			return ms.GetBuffer();
		}

		public static bool IsPotentialFormandTale(HtmlDocument taleDoc)
		{
			var polNode = taleDoc.DocumentNode.SelectSingleNode("//p[@class=\"TalerTitel\"]")
				?? taleDoc.DocumentNode.SelectSingleNode("//p[@class=\"TalerTitelMedTaleType\"]/span")
				?? taleDoc.DocumentNode.SelectSingleNode("//p[@class=\"TalerTitel\"]/span");

			if (polNode != null && polNode.InnerText.ToLower().Contains("formand"))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static void GetPolleFromTale(/*HtmlNode link*/ HtmlDocument taleDoc, out string name, out string polLink)
		{
			name = "";
			polLink = null;
			HtmlNode politiker = taleDoc.DocumentNode.SelectSingleNode("//p[@class=\"TalerTitel\"]/a")
				?? taleDoc.DocumentNode.SelectSingleNode("//p[@class=\"TalerTitelMedTaleType\"]/span/a")
				?? taleDoc.DocumentNode.SelectSingleNode("//p[@class=\"TalerTitel\"]/span/a");

			if (politiker == null)
			{
				// these occour in 2006,1
				politiker = taleDoc.DocumentNode.SelectSingleNode("//p/b/a");
			}

			bool quirksmode = false;
			if (politiker == null)
			{
				// these occour at the minimum in 2006,1 and are talks by ministers
				var thenode = taleDoc.DocumentNode.SelectSingleNode("//font/p/b");
				// sometimes it's completely fucked, 
				// see here: http://ft.dk/doc.aspx?/Samling/20061/salen/L59_BEH1_16_12_67.htm
				if (thenode != null)
				{
					string titel = thenode.InnerText;

					if (titel.ToLower().Contains("minister"))
					{
						quirksmode = true;
						var linknode = thenode.ParentNode.SelectSingleNode("a");
						if (linknode == null)
						{
							name = thenode.NextSibling.InnerText.Trim().Trim(new char[] { '(', ')', ':' });
						}
						else
						{
							name = linknode.InnerText;
							polLink = linknode.Attributes["href"].Value;
						}
					}
				}
			}

			if (politiker == null && !quirksmode)
			{
				// check that this is indeed the formand Mao
				var titelnodes = taleDoc.DocumentNode.SelectNodes("//body/font/p/b");
				if (titelnodes != null && titelnodes.OfType<HtmlNode>().Any(
					n => n.InnerText.ToLower().Contains("formand")))
				{
					//Console.WriteLine("        Formanden talte {0}", link.Attributes["href"].Value);
					name = "Formanden";
					polLink = "";
				}
				else
				{
					// try some different way, 
					// TODO, very messy control flow, should be merged with the above part
					var diftitelnode =
						taleDoc.DocumentNode.SelectSingleNode("//font/p[@class=\"TalerTitel\"]")
						?? taleDoc.DocumentNode.SelectSingleNode("//font/p[@class=\"TalerTitelMedTaleType\"]");
					if (diftitelnode != null && diftitelnode.InnerText.ToLower().Contains("formand"))
					{
						//Console.WriteLine("        Formanden talte {0}", link.Attributes["href"].Value);
						name = "Formanden";
						polLink = "";
					}
					else
					{
						//Console.WriteLine("        No politician for {0}", link.Attributes["href"].Value);
						name = "Ukendt";
						polLink = "";
					}
				}
			}
			else
				if (!quirksmode)
				{
					name = politiker.InnerText.Trim();
					polLink = politiker.Attributes["href"].Value;
				}
				else
				{
					//fall through
				}
		}

		//public static Image GetImage(string url)
		//{
		//    int BufferLength = 1024;
		//    try
		//    {
		//        WebRequest r = WebRequest.Create(url);
		//        HttpWebResponse resp = (HttpWebResponse)r.GetResponse();

		//        Image img = new Image();

		//        Stream s = resp.GetResponseStream(); //Source
		//        MemoryStream ms = new MemoryStream((int)resp.ContentLength); //Destination

		//        // from http://www.xtremevbtalk.com/showthread.php?t=263275
		//        byte[] b = new byte[BufferLength]; //Buffer
		//        int cnt = 0;
		//        do
		//        {
		//            cnt = s.Read(b, 0, BufferLength);
		//            //Write the number of bytes actually read
		//            ms.Write(b, 0, cnt);
		//        }
		//        while (cnt > 0);

		//        Binary bin = new Binary(ms.ToArray());
		//        img.Data = bin;
		//        img.ContentType = resp.ContentType;
		//        return img;
		//    }

		//    catch (Exception ex)
		//    {
		//        Console.WriteLine("Exception caught, trying to fetch: " + url + " - Exception: " + ex.ToString());
		//        return null;
		//    }

		//}

		public static FT.DB.Image GetNewImage(string url)
		{
			int BufferLength = 1024;
			try
			{
				WebRequest r = WebRequest.Create(url);
				HttpWebResponse resp = (HttpWebResponse)r.GetResponse();

				FT.DB.Image img = new FT.DB.Image();

				Stream s = resp.GetResponseStream(); //Source
				MemoryStream ms = new MemoryStream((int)resp.ContentLength); //Destination

				// from http://www.xtremevbtalk.com/showthread.php?t=263275
				byte[] b = new byte[BufferLength]; //Buffer
				int cnt = 0;
				do
				{
					cnt = s.Read(b, 0, BufferLength);
					//Write the number of bytes actually read
					ms.Write(b, 0, cnt);
				}
				while (cnt > 0);

				Binary bin = new Binary(ms.ToArray());
				img.Data = bin;
				img.ContentType = resp.ContentType;
				return img;
			}

			catch (Exception ex)
			{
				Console.WriteLine("Exception caught, trying to fetch: " + url + " - Exception: " + ex.ToString());
				return null;
			}

		}

		public static int? DownloadDocument(string url, FT.DB.P20Question question)
		{
			//make sure url is normalized
			url = url.Trim().ToLower();

			// check to see if we already have document downloaded from this Uri
			var db = new DBDataContext();
			var doc = db.Documents.SingleOrDefault(d => d.Uri == url);
			if (doc != null)
			{
				return doc.DocumentId;
			}

			try
			{
				HttpWebResponse resp = GetResponse(url, 0);

				byte[] arrBuffer = new byte[0];
				using (BinaryReader reader = new BinaryReader(resp.GetResponseStream()))
				{
					byte[] arrScratch = null;
					while ((arrScratch = reader.ReadBytes(4096)).Length > 0)
					{
						if (arrBuffer.Length == 0)
							arrBuffer = arrScratch;
						else
						{
							byte[] arrTemp = new byte[arrBuffer.Length + arrScratch.Length];
							Array.Copy(arrBuffer, arrTemp, arrBuffer.Length);
							Array.Copy(arrScratch, 0, arrTemp, arrBuffer.Length, arrScratch.Length);
							arrBuffer = arrTemp;
						}
					}
				}

				Binary bin = new Binary(arrBuffer);

				FT.DB.Document newdoc = new DB.Document();

				newdoc.Data = bin;
				newdoc.ContentType = resp.ContentType;
				newdoc.Uri = url;

				var scribdids = UpLoadToScribd(url, question);
				newdoc.ScribdId = scribdids.Item1;
				newdoc.ScribdAccessKey = scribdids.Item2;

				db.Documents.InsertOnSubmit(newdoc);
				db.SubmitChanges();

				return newdoc.DocumentId;
			}
			catch (Exception e)
			{
				return null;
			}
		}

		private static HttpWebResponse GetResponse(string url, int tries)
		{
			try
			{
				return GetResponseRec(url);
			}
			catch (Exception e)
			{
				if (tries > 10)
				{
					throw;
				}
				else
				{
					return GetResponse(url, ++tries);
				}
			}
		}

		private static HttpWebResponse GetResponseRec(string url)
		{
			WebRequest r = WebRequest.Create(url);
			r.Timeout = 100 * 1000;
			HttpWebResponse resp = (HttpWebResponse)r.GetResponse();
			return resp;

		}

		public static Tuple<int, string> UpLoadToScribd(string url, FT.DB.P20Question question)
		{
			Scribd.Net.Service.APIKey = "6qoqzj285ftfmvddexcpb";
			Scribd.Net.Service.SecretKey = "sec-6hrkkevcf77mmn34uz73csjmo7";
			Scribd.Net.Service.EnforceSigning = true;
			Scribd.Net.Service.PublisherID = "pub-82439046238225493803";

			Scribd.Net.Document sdoc = Scribd.Net.Document.Upload(url, "pdf", false);

			sdoc.Title = "Svar: " + question.Title;
			sdoc.Save();
			
			return new Tuple<int, string> (sdoc.DocumentId, sdoc.AccessKey);
		}
	}
}
