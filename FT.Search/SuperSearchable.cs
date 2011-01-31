using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolrNet;
using FT.Search;

namespace FT.Search
{
    public class SuperSearchable : ISearchable
    {
        public IEnumerable<KeyValuePair<Searchable, IDictionary<string, ICollection<string>>>>
            SolrHightlights { get; set; }
        public string CollapseField { get; set; }
        public int CollapseCount { get; set; }
        public IEnumerable<Searchable> Searchables { get; set; }

        //Kind of inherited from child searchables.
        public string SolrCollapseId { get { return Searchables.First().SolrCollapseId; } }
        public string Title { get { return Searchables.First().Title; } }
        public string Subtitle { get { return Searchables.First().Subtitle; } }
        public string Summary { get { return Searchables.First().Summary; } }
        public CollapseEntityTypes CollapseEntityType { get { return Searchables.First().CollapseEntityType; } }
        public IEnumerable<string> SolrIds { get { return Searchables.Select(s => s.SolrId); } }
        public IEnumerable<string> DocTypes { get { return Searchables.Select(s => s.DocType); } }
        public IEnumerable<string> PoliticianNames {
            get { return Searchables.Where(s => s.PoliticianName != null).Select(s => s.PoliticianName); } }
        public IEnumerable<int> ImageIds { get { return Searchables.Select(s => s.ImageId); } }
        public IEnumerable<int> PoliticianIds { get { return Searchables.Select(s => s.PoliticianId); } }

        public static IEnumerable<SuperSearchable> Parse(ISolrQueryResults<Searchable> searchables)
        {
            var groupedSearchables = searchables.GroupBy(searchable => searchable.SolrCollapseId);

            var superSearchables = new List<SuperSearchable>();
            foreach (var gSearch in groupedSearchables)
            {
                superSearchables.Add( new SuperSearchable() { Searchables = gSearch });
            }

            if (searchables.Highlights.Count > 0)
            {
                foreach (SuperSearchable sSearchable in superSearchables)
                {
                    var ssId = sSearchable.Searchables.First().SolrCollapseId;
                    sSearchable.SolrHightlights = searchables.Highlights
                        .Where(highlight => highlight.Key.SolrCollapseId == ssId)
                        .Select(doc => doc);
                }
            }

            if (searchables.Collapsing.FieldResults != null)
            {
                foreach (SuperSearchable sSearchable in superSearchables)
                {
                    var collapseField = searchables.Collapsing.FieldResults
                        .Where(fr => fr.FieldValue == sSearchable.Searchables.First().SolrCollapseId);

                    if (collapseField.Count() > 0)
                    sSearchable.CollapseCount = collapseField.First().CollapseCount;
                }
            }
            return superSearchables;
        }

    }
}
