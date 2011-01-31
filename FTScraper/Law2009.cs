using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
//using ScrapeDB;
using FT.DB;

namespace FT.Scraper
{
	class Law2009
	{
		public static void DoChangeLawTexts(string lawtext, LawStage stage, Law law, DBDataContext db)
		{
			//if (l.proposed_law_text != null)
			//{
			var doc = GetDoc(lawtext).DocumentNode;

			var ps = doc.SelectNodes("//p").OfType<HtmlNode>();
			var gps = ps.Where(p => p.Attributes["class"] != null &&
				(
					p.Attributes["class"].Value == "Paragraf" ||
					p.Attributes["class"].Value == "CentreretParagraf" ||
					p.Attributes["class"].Value == "Stk2" ||
					p.Attributes["class"].Value == "AendringMedNummer" ||
					p.Attributes["class"].Value == "KapitelOverskrift2" ||
					p.Attributes["class"].Value == "CParagrafnummer"
				));

			Paragraph currpar = null;
			int currsectionsnr = 1;
			int chaptercounter = 1;
			LawChange currchange = null;
			LawChapter currchap = null;
			foreach (var p in gps)
			{
				if (
					p.Attributes["class"].Value == "Paragraf"
					)
				{
					// TODO, this is ignored for now 
					// -- prolly we should accomodate full paragraphs under lawchanges

					if (currchange == null)
						throw new ArgumentException();

					var sc = new SubChange()
					{
						LawChange = currchange,
						Text = p.InnerText.Kapow(),
						Number = currsectionsnr++
					};
					db.SubChanges.InsertOnSubmit(sc);
				}
				else if (
					//p.Attributes["class"].Value == "Paragraf" ||
					p.Attributes["class"].Value == "CentreretParagraf")
				{
					string numbertext;
					if (p.Attributes["class"].Value == "Paragraf")
					{
						numbertext = p.SelectSingleNode("./span").InnerText;
					}
					else
					{
						// likely centered one
						numbertext = p.InnerText;
					}

					if (numbertext.StartsWith("»"))
					{
						// this is actually a quoted paragraph, make it the first subchange
						// TODO, do the above
						//continue;
					}

					numbertext = TrimPar(numbertext);
					if (numbertext == "l")
					{
						// se here http://www.ft.dk/dokumenter/tingdok.aspx?/samling/20072/lovforslag/L33/som_fremsat.htm
						numbertext = "1";
					}
					int num;
					string letter = null;
					bool succ = int.TryParse(numbertext, out num);
					if (!succ)
					{
						bool succ2 = int.TryParse(numbertext.Split(new char[] { ' ' })[0], out num);
						if (succ2)
						{
							letter = numbertext.Split(new char[] { ' ' })[1];
						}
						else
						{
							// TODO, big missing feature in here
							continue;
							// ok, this is a borky change paragraph
							// like this one http://ft.dk/Samling/20072/lovforslag/L20/som_fremsat.htm
							// create a change and stick it on above par
							//currsectionsnr = 1;
							//lawchange c = new lawchange()
							//{

							//};
							//currchange = c;
						}
					}
					Paragraph par =
					new Paragraph()
					{
						Law = law,
						Number = num,
						Stage = (LawStage)stage,
						LawChapter = currchap,
						Letter = letter
					};

					if (p.NextSibling != null &&
						p.NextSibling.NextSibling != null &&
						(
							(p.NextSibling.NextSibling.Attributes["class"] != null &&
							p.NextSibling.NextSibling.Attributes["class"].Value == "AendringMedNummer")
						||
							(p.NextSibling.NextSibling.NextSibling != null &&
							p.NextSibling.NextSibling.NextSibling.NextSibling != null &&
							p.NextSibling.NextSibling.NextSibling.NextSibling.Attributes["class"] != null &&
							p.NextSibling.NextSibling.NextSibling.NextSibling.Attributes["class"].Value == "AendringMedNummer")
						)
						)
					{
						// go into changemode
						par.IsChange = true;
						// this text is in the paragraph, instead of the first stk
						if (p.ChildNodes.Count < 3)
						{
							par.ChangeText = p.NextSibling.NextSibling.InnerText.Kapow();
						}
						else
						{
							par.ChangeText = p.ChildNodes[2].InnerText.Trim().Kapow();
						}
					}
					else
					{
						par.IsChange = false;
						currchange = null; // reset the fucker
						// the paragraph also contains the text of stk 1

						string sectiontxt = null;
						if (p.ChildNodes.Count < 2)
						{
							// this suggest a paragraph with no initial unnumbered section
							// we set sectionnr to zero to get the right numbers
							// there's an example in paragraf 9 here 
							// http://ft.dk/Samling/20072/lovforslag/L175/som_fremsat.htm
							currsectionsnr = 0;
							db.Paragraphs.InsertOnSubmit(par);
							currpar = par;
							continue;
							//sectiontxt = p.ChildNodes[1].InnerText.Trim();
						}
						else if (p.ChildNodes.Count < 3)
						{
							sectiontxt = p.ChildNodes[1].InnerText.Trim();
						}
						else
						{
							sectiontxt = p.ChildNodes[2].InnerText.Trim(); // to get the pure text
						}
						currsectionsnr = 1;
						if (par == null)
							throw new ArgumentException();

						Section s = new Section()
						{
							Paragraph = par,
							Number = currsectionsnr,
							Text = sectiontxt.Kapow()
						};

						CheckforList(p, s);
						db.Sections.InsertOnSubmit(s);
					}
					currpar = par;
					db.Paragraphs.InsertOnSubmit(par);

				}
				// check that we are not in change mode
				else if (p.Attributes["class"].Value == "Stk2")
				{
					//string numbertext = p.SelectSingleNode("./span").InnerText;
					//numbertext = numbertext.Trim(new char[] { 'S', 't', 'k', '.', ' ' }).Trim();
					//int num = int.Parse(numbertext);

					string sectiontxt = GetSectionText(p);

					if (string.IsNullOrEmpty(sectiontxt))
					{
						// this suggests the text is hidden in different div
						if (p.NextSibling.NextSibling.ChildNodes.Count > 1)
						{
							sectiontxt = p.NextSibling.NextSibling.ChildNodes[1].InnerText;
						}
						else
						{
							// this one has pretty weird form
							// http://ft.dk/Samling/20072/lovforslag/L94/som_fremsat.htm
							sectiontxt = p.InnerText;
						}
					}

					if (currchange != null)
					{
						currsectionsnr++;
						
						var sub = new SubChange()
						{
							Number = currsectionsnr,
							Text = sectiontxt.Kapow(),
							LawChange = currchange,

						};

						db.SubChanges.InsertOnSubmit(sub);
					}
					else
					{
						currsectionsnr++;
						if (currpar == null)
							throw new ArgumentException("wrong state");

						var s = new Section()
						{
							Text = sectiontxt.Kapow(),
							Number = currsectionsnr,
							Paragraph = currpar
						};

						db.Sections.InsertOnSubmit(s);
					}
				}
				else if (p.Attributes["class"].Value == "AendringMedNummer")
				{
					string numbertext = p.SelectSingleNode("./span").InnerText;
					numbertext = numbertext.Trim(new char[] { '.' }).Trim();
					int num;
					bool succ = int.TryParse(numbertext, out num);
					if (succ)
					{
						if (currpar == null)
							throw new ArgumentException();

						var change = new LawChange()
						{
							// interesting formatting in here
							//ChangeText = p.InnerHtml,
							Number = num,
							Paragraph = currpar,
							NoformChangeText = p.InnerText.Kapow()

						};

						db.LawChanges.InsertOnSubmit(change);
						currchange = change;
					}
					else
					{
						// happens in paragraph 6 here: 
						// http://ft.dk/doc.aspx?/Samling/20072/lovforslag/L187/som_vedtaget.htm
						Console.WriteLine("the problem");
					}
				}
				else if (p.Attributes["class"].Value == "KapitelOverskrift2")
				{
					var chap = new LawChapter()
					{
						//Law = l,
						Number = chaptercounter++,
						Titel = p.InnerText,
						Stage = (byte)stage // this is fremsat

					};
					db.LawChapters.InsertOnSubmit(chap);
					currchap = chap;
				}
				else
				{
					// unrelated paragraph, do nothing
				}
			}
			//}
		}


		public static void DoNonChangeLawTexts(string lawtext, LawStage stage, Law law, DBDataContext db)
		{
			var doc = GetDoc(lawtext).DocumentNode;

			var ps = doc.SelectNodes("//p").OfType<HtmlNode>();
			var gps = ps.Where(p => p.Attributes["class"] != null &&
				(
					p.Attributes["class"].Value == "Paragraf" ||
					p.Attributes["class"].Value == "CentreretParagraf" ||
					p.Attributes["class"].Value == "Stk2" ||
					p.Attributes["class"].Value == "AendringMedNummer" ||
					p.Attributes["class"].Value == "KapitelOverskrift2"
				));

			Paragraph currpar = null;
			int currsectionsnr = 1;
			int chaptercounter = 1;
			LawChange currchange = null;
			LawChapter currchap = null;
			foreach (var p in gps)
			{
				if (p.Attributes["class"].Value == "Paragraf" ||
					p.Attributes["class"].Value == "CentreretParagraf")
				{
					string numbertext;
					if (p.Attributes["class"].Value == "Paragraf")
					{
						var node = p.SelectSingleNode("./span");
						if (node != null)
						{
							numbertext = node.InnerText;
						}
						else
						{
							// TODO, bug in this one
							// http://www.ft.dk/dokumenter/tingdok.aspx?/samling/20072/lovforslag/L20/som_fremsat.htm
							continue;
						}
					}
					else
					{
						numbertext = p.InnerText;
					}

					if (numbertext.StartsWith("»"))
					{
						// this is actually a quoted paragraph, make it the first subchange
						// TODO, do the above
						continue;
					}

					numbertext = TrimPar(numbertext);
					int num;
					string letter = null;
					bool succ = int.TryParse(numbertext, out num);
					if (!succ)
					{
						bool succ2 = int.TryParse(numbertext.Split(new char[] { ' ' })[0], out num);
						if (succ2)
						{
							letter = numbertext.Split(new char[] { ' ' })[1];
						}
						else
						{
							// TODO, big missing feature in here
							continue;
							// ok, this is a borky change paragraph
							// like this one http://ft.dk/Samling/20072/lovforslag/L20/som_fremsat.htm
							// create a change and stick it on above par
							//currsectionsnr = 1;
							//lawchange c = new lawchange()
							//{

							//};
							//currchange = c;
						}
					}
					Paragraph par =
						new Paragraph()
						{
							Law = law,
							Number = num,
							Stage = (LawStage)stage,
							LawChapter = currchap,
							Letter = letter
						};

					if (p.NextSibling != null &&
						p.NextSibling.NextSibling != null &&
						(
							(p.NextSibling.NextSibling.Attributes["class"] != null &&
							p.NextSibling.NextSibling.Attributes["class"].Value == "AendringMedNummer")
						||
							(p.NextSibling.NextSibling.NextSibling != null &&
							p.NextSibling.NextSibling.NextSibling.NextSibling != null &&
							p.NextSibling.NextSibling.NextSibling.NextSibling.Attributes["class"] != null &&
							p.NextSibling.NextSibling.NextSibling.NextSibling.Attributes["class"].Value == "AendringMedNummer")
						)
						)
					{
						// go into changemode
						par.IsChange = true;
						// this text is in the paragraph, instead of the first stk
						if (p.ChildNodes.Count < 3)
						{
							par.ChangeText = p.NextSibling.NextSibling.InnerText.Kapow();
						}
						else
						{
							par.ChangeText = p.ChildNodes[2].InnerText.Trim().Kapow();
						}
					}
					else
					{
						par.IsChange = false;
						currchange = null; // reset the fucker
						// the paragraph also contains the text of stk 1

						string sectiontxt = null;
						if (p.ChildNodes.Count < 2)
						{
							// this suggest a paragraph with no initial unnumbered section
							// we set sectionnr to zero to get the right numbers
							// there's an example in paragraf 9 here 
							// http://ft.dk/Samling/20072/lovforslag/L175/som_fremsat.htm
							currsectionsnr = 0;
							currpar = par;
							db.Paragraphs.InsertOnSubmit(par);
							continue;
							//sectiontxt = p.ChildNodes[1].InnerText.Trim();
						}
						else if (p.ChildNodes.Count < 3)
						{
							sectiontxt = p.ChildNodes[1].InnerText.Trim();
						}
						else
						{
							sectiontxt = p.ChildNodes[2].InnerText.Trim(); // to get the pure text
						}
						currsectionsnr = 1;

						if (par == null)
							throw new ArgumentException();

						Section s = new Section()
						{
							Paragraph = par,
							Number = currsectionsnr,
							Text = sectiontxt.Kapow()
						};

						CheckforList(p, s);
						db.Sections.InsertOnSubmit(s);
					}
					currpar = par;
					db.Paragraphs.InsertOnSubmit(par);

				}
				// check that we are not in change mode
				else if (p.Attributes["class"].Value == "Stk2")
				{
					//string numbertext = p.SelectSingleNode("./span").InnerText;
					//numbertext = numbertext.Trim(new char[] { 'S', 't', 'k', '.', ' ' }).Trim();
					//int num = int.Parse(numbertext);

					string sectiontxt = GetSectionText(p);

					if (currchange != null)
					{
						currsectionsnr++;
						var sub = new SubChange()
						{
							Number = currsectionsnr,
							Text = sectiontxt.Kapow(),
							LawChange = currchange
						};

						db.SubChanges.InsertOnSubmit(sub);
					}
					else
					{
						currsectionsnr++;

						if (currpar == null)
							throw new ArgumentException();

						var s = new Section()
						{
							Text = sectiontxt.Kapow(),
							Number = currsectionsnr,
							Paragraph = currpar
						};

						db.Sections.InsertOnSubmit(s);
					}
				}
				else if (p.Attributes["class"].Value == "AendringMedNummer")
				{
					string numbertext = p.SelectSingleNode("./span").InnerText;
					numbertext = numbertext.Trim(new char[] { '.' }).Trim();
					int num;
					var succ = int.TryParse(numbertext, out num);
					if (succ)
					{
						if (currpar == null)
							throw new ArgumentException();

						var change = new LawChange()
						{
							// interesting formatting in here
							//ChangeText = p.InnerHtml,
							Number = num,
							Paragraph = currpar,
							NoformChangeText = p.InnerText.Kapow()
						};

						db.LawChanges.InsertOnSubmit(change);
						currchange = change;
					}
				}
				else if (p.Attributes["class"].Value == "KapitelOverskrift2")
				{
					var chap = new LawChapter()
					{
						//Law = l,
						Number = chaptercounter++,
						Titel = p.InnerText,
						Stage = (byte)stage

					};
					db.LawChapters.InsertOnSubmit(chap);
					currchap = chap;
				}
				else
				{
					// unrelated paragraph, do nothing
				}
			}
			//}
		}

		private static string TrimPar(string numbertext)
		{
			return numbertext.Trim(new char[] { '§', ' ', '.' }).
						Replace("&#160;", "").
						Replace("&nbsp;", "").
						Trim();
		}

		private static string GetSectionText(HtmlNode p)
		{
			string sectiontxt = null;
			if (p.ChildNodes.Count < 2)
			{
				sectiontxt = p.InnerText;
			}
			else if (p.ChildNodes.Count < 3)
			{
				sectiontxt = p.ChildNodes[1].InnerText.Trim();
			}
			else
			{
				sectiontxt = p.ChildNodes[2].InnerText.Trim(); // to get the pure text
			}
			return sectiontxt;
		}

		private static HtmlDocument GetDoc(string html)
		{
			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(html);
			return doc;
		}

		private static void CheckforList(HtmlNode p, Section s)
		{
			// check if two elements over, we have a div
			if (p.NextSibling != null &&
				p.NextSibling.NextSibling != null &&
				p.NextSibling.NextSibling.HasChildNodes &&
				p.NextSibling.NextSibling.ChildNodes.Count > 1 &&
				(p.NextSibling.NextSibling.ChildNodes[1].Name == "div" ||
				p.NextSibling.NextSibling.ChildNodes[1].Name == "p")
				//&&
				//p.NextSibling.NextSibling.ChildNodes[1].
				//    SelectNodes("./p[@class=\"Liste1\"]") != null
				//&&
				//p.NextSibling.NextSibling.NextSibling != null &&
				//p.NextSibling.NextSibling.NextSibling.NextSibling != null &&
				//p.NextSibling.NextSibling.NextSibling.NextSibling.Name == "div"
			)
			{
				StringBuilder res = new StringBuilder("<ol>");
				if (p.NextSibling.NextSibling.ChildNodes[1].
					SelectNodes("./p[@class=\"Liste1\"]") == null)
				{
					// this is one that pretends to be a list but isn't
					// http://ft.dk/Samling/20072/lovforslag/L178/som_fremsat.htm

					s.Text += " " + p.NextSibling.NextSibling.InnerText.Kapow();
				}
				else
				{
					foreach (var thep in
						p.NextSibling.NextSibling.ChildNodes[1].
						SelectNodes("./p[@class=\"Liste1\"]").OfType<HtmlNode>())
					{
						res.Append("<li>" + thep.InnerText.Trim() + "</li>");
						if (
							thep.NextSibling != null &&
							thep.NextSibling.NextSibling != null &&
							thep.NextSibling.NextSibling.HasChildNodes &&
							thep.NextSibling.NextSibling.ChildNodes.Count > 1 &&
							thep.NextSibling.NextSibling.ChildNodes[1].Name == "div"
							//thep.NextSibling != null &&
							//thep.NextSibling.NextSibling != null &&
							//thep.NextSibling.NextSibling.NextSibling != null &&
							//thep.NextSibling.NextSibling.NextSibling.NextSibling != null &&
							//thep.NextSibling.NextSibling.NextSibling.NextSibling.Name == "div"
							)
						{
							if (thep.NextSibling.NextSibling.ChildNodes[1].
								SelectNodes("./p[@class=\"Liste2\"]") != null)
							{
								res.Append("<ol>");
								foreach (var thethep in
									thep.NextSibling.NextSibling.ChildNodes[1].
									SelectNodes("./p[@class=\"Liste2\"]").OfType<HtmlNode>())
								{
									res.Append("<li>" + thethep.InnerText.Trim().Kapow() + "</li>");
								}
								res.Append("<ol>");
							}
							else
							{
								// TODO, bug here, see this thing:
								//http://ft.dk/Samling/20072/lovforslag/L64/som_fremsat.htm
								continue;
							}
						}
					}
					res.Append("</ol>");
					s.Text += res.ToString();
				}
			}
		}
	}

	public static class Extension
	{
		public static string Kapow(this string s)
		{
			return s.Replace("\r\n", " ");
		}
	}
}
