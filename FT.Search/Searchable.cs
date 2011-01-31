using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolrNet.Attributes;
using SolrNet.Mapping;
using SolrNet;
using FT.DB;
using FT.Search.Helpers;
using System.Text.RegularExpressions;

namespace FT.Search
{
    public enum EntityTypes
    {
        Paragraf,
        SubChange,
        LawChange,
        Speech,
        Section,
        Politician
    }

    public enum CollapseEntityTypes
    {
        Law,
        Politician
    }

    public class Searchable : ISearchable
    {
        public Searchable()
        {
        }

        public LawStage LawStageEnum
        {
            get { return (LawStage)Enum.Parse(typeof(LawStage), (Stage).ToString()); }
        }

        public int CollapseEntityId
        {
            get { return int.Parse(Regex.Match(SolrCollapseId, @"\d+").Value); }
        }

        public CollapseEntityTypes CollapseEntityType
        {
            get { return (CollapseEntityTypes)Enum.Parse(typeof(CollapseEntityTypes), (Regex.Match(SolrCollapseId, @"\D+").Value)); }
        }

        public EntityTypes EntityType
        {
            get
            {
                EntityTypes internalDocType;
                string entityId = Regex.Match(SolrId, @"\D+").Value;
                bool success = Enum.TryParse(entityId, true, out internalDocType);
                return internalDocType;
            }
        }

        public int EntityId
        {
            get { return int.Parse(Regex.Match(SolrId, @"\d+").Value); }
        }

        [SolrField("collapse_id")]
        public string SolrCollapseId { get; set; }

        [SolrUniqueKey("solr_id")]
        public string SolrId { get; set; }

        [SolrField("doc_type")]
        public string DocType { get; set; }

        [SolrField("title")]
        public string Title { get; set; }

        [SolrField("subtitle")]
        public string Subtitle { get; set; }

        [SolrField("content")]
        public string Content { get; set; }

        [SolrField("politician_name")]
        public string PoliticianName { get; set; }

        [SolrField("politician_id")]
        public int PoliticianId { get; set; }

        [SolrField("proposed_year")]
        public int ProposedYear { get; set; }

        [SolrField("ministry_name")]
        public string MinistryName { get; set; }

        [SolrField("party")]
        public string PartyName { get; set; }

        [SolrField("summary")]
        public string Summary { get; set; }

        [SolrField("stage")]
        public int Stage { get; set; }

        [SolrField("fct_stage")]
        public string StageHumanized { get; set; }

        [SolrField("image_id")]
        public int ImageId { get; set; }

        [SolrField("fct_doc_type")]
        public string DocTypeHumanized { get; set; }
        public string DefaultSearchFilter
        {
            get { return "doc_type=" + DocType; }
        }


    }

}
