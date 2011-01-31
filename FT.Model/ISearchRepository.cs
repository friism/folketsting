using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using FT.DB;
using System.Web.Mvc;

namespace FT.Model
{
	public interface ISearchRepository
	{
		//IEnumerable<IEnumerable<SearchSubResult>> Find(string query, int limit);
		//IEnumerable<IEnumerable<SearchSubResult>> AdvancedFind(Query query, int limit);
	}

	public class SearchRepository : Repository, ISearchRepository
	{
	//    private Dictionary<int, Law> lawcache = new Dictionary<int, Law>();
	//    private Dictionary<int, P20Question> p20cache = new Dictionary<int, P20Question>();
	//    private Dictionary<int, Speech> speechcache = new Dictionary<int, Speech>();

	//    private Law GetLaw(int? lawid)
	//    {
	//        if (!lawcache.ContainsKey(lawid.Value))
	//        {
	//            Law l = DB.Laws.Single(_ => _.LawId == lawid);
	//            lawcache.Add(lawid.Value, l);
	//            return l;
	//        }
	//        return lawcache[lawid.Value];
	//    }

	//    private P20Question GetP20(int? p20id)
	//    {
	//        if (!p20cache.ContainsKey(p20id.Value))
	//        {
	//            P20Question p = DB.P20Questions.Single(_ => _.P20QuestionId == p20id);
	//            p20cache.Add(p20id.Value, p);
	//            return p;
	//        }
	//        return p20cache[p20id.Value];
	//    }

	//    private Speech GetSpeech(int? speechid)
	//    {
	//        if (!speechcache.ContainsKey(speechid.Value))
	//        {
	//            Speech s = DB.Speeches.Single(_ => _.SpeechId == speechid);
	//            speechcache.Add(speechid.Value, s);
	//            return s;
	//        }
	//        return speechcache[speechid.Value];
	//    }

		//public IEnumerable<IEnumerable<SearchSubResult>> AdvancedFind(Query fq, int limit)
		//{
		//    string query = fq.Text.Trim().Split(' ').Select(_ => _.Trim()).Aggregate((a, b) => a + " AND " + b);

		//    // slightly nasty hack
		//    if (string.IsNullOrEmpty(fq.PolName))
		//        fq.PolName = null;
		//    else
		//        fq.PolName = string.Format("%{0}%", fq.PolName);

		//    var preqq =
		//        (fq.Type == null || fq.Type == SearchResType.Question)
		//        ? DB.P20QuestionQuestionSearch(query, fq.PolName, fq.From, fq.To).OrderByDescending(_ => _.Rank).Take(limit).ToList()
		//        : new List<P20QuestionQuestionSearchResult> { };
		//    var qq = preqq.Select(_ => _ as IProtoSearchResultItem);
			
		//    var preqa =
		//        (fq.Type == null || fq.Type == SearchResType.Answer)
		//        ? DB.P20QuestionAnswerSearch(query, fq.PolName, fq.From, fq.To).OrderByDescending(_ => _.Rank).Take(limit).ToList()
		//        : new List<P20QuestionAnswerSearchResult> { };
		//    var qa = preqa.Select(_ => _ as IProtoSearchResultItem);

		//    var pred1 =
		//        (fq.Type == null || fq.Type == SearchResType.FirstDelib)
		//        ? DB.SpeechSearch(query, 1, fq.PolName, fq.From, fq.To).OrderByDescending(_ => _.Rank).Take(limit).ToList()
		//        : new List<SpeechSearchResult> { };
		//    var d1 = pred1.Select(_ => _ as IProtoSearchResultItem);

		//    var pred2 =
		//        (fq.Type == null || fq.Type == SearchResType.SecondDelib)
		//        ? DB.SpeechSearch(query, 2, fq.PolName, fq.From, fq.To).OrderByDescending(_ => _.Rank).Take(limit).ToList()
		//        : new List<SpeechSearchResult> { };
		//    var d2 = pred2.Select(_ => _ as IProtoSearchResultItem);

		//    var pred3 =
		//        (fq.Type == null || fq.Type == SearchResType.ThirdDelib)
		//        ? DB.SpeechSearch(query, 3, fq.PolName, fq.From, fq.To).OrderByDescending(_ => _.Rank).Take(limit).ToList()
		//        : new List<SpeechSearchResult> { };
		//    var d3 = pred3.Select(_ => _ as IProtoSearchResultItem);

		//    // note that these are only included when politician is null
		//    var prelt1 =
		//        ((fq.Type == null || fq.Type == SearchResType.PropLT) && fq.PolName == null)
		//        ? DB.LawTextPropSearch(query, fq.From, fq.To).OrderByDescending(_ => _.Rank).Take(limit).ToList()
		//        : new List<LawTextPropSearchResult> { };
		//    var lt1 = prelt1.Select(_ => _ as IProtoSearchResultItem);
			
		//    var prelt2 =
		//        ((fq.Type == null || fq.Type == SearchResType.SecondLT) && fq.PolName == null)
		//        ? DB.LawTextSecondSearch(query, fq.From, fq.To).OrderByDescending(_ => _.Rank).Take(limit).ToList()
		//        : new List<LawTextSecondSearchResult> { };
		//    var lt2 = prelt2.Select(_ => _ as IProtoSearchResultItem);
			
		//    var prelt3 =
		//        ((fq.Type == null || fq.Type == SearchResType.PassLT) && fq.PolName == null)
		//        ? DB.LawTextPassedSearch(query, fq.From, fq.To).OrderByDescending(_ => _.Rank).Take(limit).ToList()
		//        : new List<LawTextPassedSearchResult> { };
		//    var lt3 = prelt3.Select(_ => _ as IProtoSearchResultItem);

		//    var ordered =
		//        (
		//        from x in d1.Concat(d2).Concat(d3).Concat(qq).Concat(qa).Concat(lt1).Concat(lt2).Concat(lt3)
		//        group x by x.GetGroupId() into g
		//        select new { id = g.Key, rank = g.Average(_ => _.Rank), type = g.First().GetType().Name }
		//          ).OrderByDescending(_ => _.rank).Take(limit);

		//    foreach (var x in ordered)
		//    {
		//        switch (x.type)
		//        {
		//            //case "SpeechSearch2Result":
		//            case "SpeechSearchResult":
		//                {
		//                    yield return (from l in pred1
		//                                  where l.DeliberationId == x.id
		//                                  orderby l.Rank descending
		//                                  select new SearchSubResult
		//                                  {
		//                                      Item = GetLaw(l.LawId),
		//                                      Type = SearchResType.FirstDelib,
		//                                      SpeechId = l.SpeechId,
		//                                      Speech = GetSpeech(l.SpeechId)
		//                                  } as SearchSubResult).Concat(
		//                                     from l in pred2
		//                                     where l.DeliberationId == x.id
		//                                     orderby l.Rank descending
		//                                     select new SearchSubResult
		//                                     {
		//                                         Item = GetLaw(l.LawId),
		//                                         Type = SearchResType.SecondDelib,
		//                                         SpeechId = l.SpeechId,
		//                                         Speech = GetSpeech(l.SpeechId)
		//                                     } as SearchSubResult).Concat(
		//                                     from l in pred3
		//                                     where l.DeliberationId == x.id
		//                                     orderby l.Rank descending
		//                                     select new SearchSubResult
		//                                     {
		//                                         Item = GetLaw(l.LawId),
		//                                         Type = SearchResType.ThirdDelib,
		//                                         SpeechId = l.SpeechId,
		//                                         Speech = GetSpeech(l.SpeechId)
		//                                     } as SearchSubResult);
		//                    break;
		//                }
		//            case "P20QuestionQuestionSearchResult":
		//                {
		//                    yield return (from q in preqq
		//                                  where q.P20QuestionId == x.id
		//                                  orderby q.Rank descending
		//                                  select new SearchSubResult
		//                                  {
		//                                      Item = GetP20(q.P20QuestionId),
		//                                      Type = SearchResType.Question,
		//                                  } as SearchSubResult).Concat
		//                                 (
		//                                 from q in preqa
		//                                 where q.P20QuestionId == x.id
		//                                 orderby q.Rank descending
		//                                 select new SearchSubResult
		//                                 {
		//                                     Item = GetP20(q.P20QuestionId),
		//                                     Type = SearchResType.Answer,
		//                                 } as SearchSubResult
		//                                 );
		//                    break;
		//                }
		//            case "P20QuestionAnswerSearchResult":
		//                {
		//                    yield return (from q in preqq
		//                                  where q.P20QuestionId == x.id
		//                                  orderby q.Rank descending
		//                                  select new SearchSubResult
		//                                  {
		//                                      Item = GetP20(q.P20QuestionId),
		//                                      Type = SearchResType.Question,
		//                                  } as SearchSubResult).Concat
		//                                 (
		//                                 from q in preqa
		//                                 where q.P20QuestionId == x.id
		//                                 orderby q.Rank descending
		//                                 select new SearchSubResult
		//                                 {
		//                                     Item = GetP20(q.P20QuestionId),
		//                                     Type = SearchResType.Answer,
		//                                 } as SearchSubResult
		//                                 );
		//                    break;
		//                }
		//            case "LawTextPropSearchResult":
		//                {
		//                    yield return new
		//                        List<SearchSubResult> { 
		//                        new SearchSubResult { 
		//                            Item = GetLaw(prelt1.Single(_ => _.MaxParagraphId == x.id).LawId),
		//                            Type = SearchResType.PropLT
		//                        }
		//                    };
		//                    break;
		//                }
		//            case "LawTextSecondSearchResult":
		//                {
		//                    yield return new
		//                        List<SearchSubResult> { 
		//                        new SearchSubResult { 
		//                            Item = GetLaw(prelt2.Single(_ => _.MaxParagraphId == x.id).LawId),
		//                            Type = SearchResType.SecondLT
		//                        }
		//                    };
		//                    break;
		//                }
		//            case "LawTextPassedSearchResult":
		//                {
		//                    yield return new
		//                        List<SearchSubResult> { 
		//                        new SearchSubResult { 
		//                            Item = GetLaw(prelt3.Single(_ => _.MaxParagraphId == x.id).LawId),
		//                            Type = SearchResType.PassLT
		//                        }
		//                    };
		//                    break;
		//                }
		//            default: throw new ArgumentException(x.GetType().Name);
		//        }

		//    }
		//}

		//public IEnumerable<IEnumerable<SearchSubResult>> Find(string query, int limit)
		//{
		//    return AdvancedFind(new Query { Text = query }, limit);
		//}
	}

	public enum SearchResType { FirstDelib, SecondDelib, ThirdDelib, PropLT, SecondLT, PassLT, Question, Answer };

	//public class SearchSubResult : ISearchResultItem
	//{
	//    public SearchResType Type { get; set; }
	//    public object Item { get; set; }
	//    public int? SpeechId { get; set; }
	//    public Speech Speech { get; set; }
	//}

    public class Query
    {
		//public string Text { get; set; }
		//public string PolName { get; set; }
		//public DateTime? From { get; set; }
		//public DateTime? To { get; set; }
		//public string FromString { get; set; }
		//public string ToString { get; set; }
		//public SearchResType? Type { get; set; }
		//public IEnumerable<SelectListItem> SelectTypes
		//{
		//    get
		//    {
		//        return new List<SelectListItem>
		//        {
		//            new SelectListItem { Value = ((int) SearchResType.Answer).ToString(), Text = "§20 Svar"},
		//            new SelectListItem { Value = ((int) SearchResType.Question).ToString(), Text = "§20 Spørgsmål"},
		//            new SelectListItem { Value = ((int) SearchResType.FirstDelib).ToString(), Text = "Debat, 1. behandling"},
		//            new SelectListItem { Value = ((int) SearchResType.SecondDelib).ToString(), Text = "Debat, 2. behandling"},
		//            new SelectListItem { Value = ((int) SearchResType.ThirdDelib).ToString(), Text = "Debat, 3. behandling"},
		//            new SelectListItem { Value = ((int) SearchResType.PropLT).ToString(), Text = "Lovtekst, foreslået"},
		//            new SelectListItem { Value = ((int) SearchResType.SecondLT).ToString(), Text = "Lovtekst, 2. behandling"},
		//            new SelectListItem { Value = ((int) SearchResType.PassLT).ToString(), Text = "Lovtekst, vedtaget"},
		//        };
		//    }
		//}
    }
}
