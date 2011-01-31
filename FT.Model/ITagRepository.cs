using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using FT.DB;
//using ScrapeDB.Model;

namespace FT.Model
{
    public interface ITagRepository
    {
        IQueryable<string> Find(string query, int limit);
        //IEnumerable<ISearchResultItem> HottestByTagName(string name, int limit);
        IQueryable<string> UserTags(int id, int userid, ContentType type);
        IEnumerable<string> TopSiteShuffled();
        void InsertTags(IEnumerable<string> tags, int userid, int id, ContentType type);
        Dictionary<string, int> CountedTags(int elementid, ContentType type);
        IEnumerable<string> TopShuffled(int elementid, ContentType type);
        void DeleteTagsByUser(int id, ContentType type, int p);
        Dictionary<string, int> TagCloudData();
        int TotalTags();
    }

    public class TagRepository : Repository, ITagRepository
    {
        public Dictionary<string, int> TagCloudData()
        {
            Dictionary<string, int> res = new Dictionary<string,int>();
            var tags = from t in DB.Tags
                       group t by t.TagName into g
                       where g.Count() > 5
                       select new {g.Key, Count = g.Count()};
            foreach (var tag in tags)
            {
                res.Add(tag.Key, tag.Count);
            }
            return res;
        }

        public int TotalTags()
        {
            return (from t in DB.Tags
                    group t by new { t.ContentId, t.ContentType } into g
                    select g).Count();
        }

        public IQueryable<string> Find(string query, int limit)
        {
            return (from t in DB.Tags
                    where t.TagName.StartsWith(query)
                    orderby t.TagName
                    select t.TagName).Distinct().Take(limit);
        }

        public IDictionary<string, string> HottestByTagName(string name, int limit)
        {
            return new Dictionary<string, string>();
            //var laws = (from l in DB.Laws
            //            join t in DB.Tags on l.LawId equals t.ContentId
            //            where t.ContentType == ContentType.Law && t.TagName == name
            //            group l by l.LawId into ll
            //            orderby ll.Count() descending
            //            select new { ll.Key, Count = ll.Count() }).Take(limit).
            //            Select(i => new
            //            {
            //                Item = DB.Laws.Single(_ => _.LawId == i.Key)
            //                    as ISearchResultItem,
            //                Count = i.Count
            //            }).ToList();

            //var qs = (from q in DB.P20Questions
            //          join t in DB.Tags on q.P20QuestionId equals t.ContentId
            //          where t.ContentType == ContentType.P20Question && t.TagName == name
            //          group q by q.P20QuestionId into qq
            //          orderby qq.Count() descending
            //          select new { qq.Key, Count = qq.Count() }).Take(limit).
            //            Select(i => new
            //            {
            //                Item = DB.P20Questions.Single(_ => _.P20QuestionId == i.Key)
            //                    as ISearchResultItem,
            //                Count = i.Count
            //            }).ToList();
            //var rs = laws.Concat(qs).OrderByDescending(_ => _.Count).Take(limit);

            //return rs.Select(_ => _.Item);
        }

        private IQueryable<Tag> ByLaw(int lawid)
        {
            return from t in DB.Tags
                   where t.ContentType == ContentType.Law && t.ContentId == lawid
                   select t;
        }

        public Dictionary<string, int> CountedTags(int elementid, ContentType type)
        {
            var res = new Dictionary<string, int>();
            var thetags = from t in DB.Tags
                          where t.ContentType == type && t.ContentId == elementid
                          group t by t.TagName into _
                          select new { tag = _.Key, count = _.Count() };

            foreach (var t in thetags)
            {
                res.Add(t.tag, t.count);
            }

            return res;
        }

        //public Dictionary<string, int> CountedTagsByLaw(int lawid)
        //{
        //    var res = new Dictionary<string, int>();
        //    var thetags = from t in ByLaw(lawid)
        //                  group t by t.TagName into _
        //                  select new { tag = _.Key, count = _.Count() };

        //    foreach (var t in thetags)
        //    {
        //        res.Add(t.tag, t.count);
        //    }

        //    return res;
        //}

        public IQueryable<string> UserTags(int id, int userid, ContentType type)
        {
            return (from t in DB.Tags
                    where t.UserId == userid && t.ContentId == id && t.ContentType == type
                    select t.TagName).Distinct();
        }

        public IEnumerable<string> TopShuffled(int elementid, ContentType type)
        {
            return (from t in DB.Tags
                    where t.ContentId == elementid && t.ContentType == type
                    group t by t.TagName into _
                    orderby _.Count() descending
                    select _.Key).Take(5).Shuffle();
        }

        public IEnumerable<string> TopSiteShuffled()
        {
            return (from t in DB.Tags
                    group t by t.TagName into _
                    orderby _.Count() descending
                    select _.Key).Take(20).TakeRandom(5);
        }

        public void DeleteTagsByUser(int id, ContentType type, int userid)
        {
            DB.Tags.DeleteAllOnSubmit(
                from t in DB.Tags
                where t.UserId == userid && t.ContentType == type && t.ContentId == id
                select t
            );
            DB.SubmitChanges();
        }

        public void InsertTags(IEnumerable<string> tags, int userid, int id, ContentType type)
        {
            DB.Tags.InsertAllOnSubmit(
            tags.Select(
                _ => new Tag()
                {
                    ContentId = id,
                    ContentType = type,
                    Date = DateTime.Now,
                    TagName = _,
                    UserId = userid,
                }
            )
            );
            DB.SubmitChanges();
        }
    }
}
