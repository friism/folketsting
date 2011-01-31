using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SolrNet;
using SolrNet.Impl;
using Microsoft.Practices.ServiceLocation;
using System.Xml;
using System.Reflection;
using System.Globalization;
using SolrNet.Mapping;
using SolrNet.Utils;
using SolrNet.Commands.Parameters;

namespace FT.Search.Tests
{
    [TestClass]
    public class SearchableIntegrationTest
    {
        ISolrOperations<Searchable> solr_searchable;

        [AssemblyInitialize()]
        public static void InitSolr(TestContext testcontext)
        {
            Startup.Init<Searchable>("http://localhost:8983/solr");
        }

        private TestContext testContextInstance;
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestInitialize]
        public void FixtureSetup()
        {
            solr_searchable = ServiceLocator.Current.GetInstance<ISolrOperations<Searchable>>();
        }

        [TestMethod]
        public void ShouldAddAndGetLaw()
        {
            Searchable _shab = new Searchable();
            
            _shab.LawId = 123;
            _shab.SolrId = "Law123";
            _shab.Title = "Money for everybody.";
            _shab.DocType = "Law";

            solr_searchable.Add(_shab).Commit();

            var results = solr_searchable.Query("solr_id:Law123");
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Money for everybody.", results.First().Title);
        }

        //Is dependent on data imported externally.
        [TestMethod]
        public void ShouldFetchAndParseDocWithContent()
        {
            var queryOptions = new QueryOptions
            {
                FilterQueries = new[] {
                    new SolrQueryByField("solr_id", "paragraph19338"),
                    new SolrQueryByField("doc_type", "Paragraph")
                }
            };

            var results = solr_searchable.Query(SolrQuery.All, queryOptions);
            Assert.AreEqual(1, results.NumFound);
        }
    }
}
