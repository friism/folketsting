using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FT.DB;
using System.Threading;

namespace FT.Scraper.Tests
{
    [TestClass]
    public class ScribdTest
    {
        [TestMethod]
        public void TestUploadFail()
        {
            P20Question q = new P20Question { Title = "Foo2" };
            Util.UpLoadToScribd(
                "http://www.ft.dk/samling/20091/spoergsmaal/s2094/svar/721349/855770.pdf",
                q);
        }

        [TestMethod]
        public void DeleteAllDocs()
        {
            var db = new DBDataContext();

            foreach (var d in db.Documents)
            {
                if (d.ScribdId.Value != 35896972)
                {
                    Scribd.Net.Document.Delete(d.ScribdId.Value);
                }
                db.Documents.DeleteOnSubmit(d);
            }
            db.SubmitChanges();

        }

        [TestMethod]
        public void DeleteAllDocsViaScribd()
        {
            int pagesize = 1000;

            Scribd.Net.Service.APIKey = "6qoqzj285ftfmvddexcpb";
            Scribd.Net.Service.SecretKey = "sec-6hrkkevcf77mmn34uz73csjmo7";
            Scribd.Net.Service.EnforceSigning = true;
            Scribd.Net.Service.PublisherID = "pub-82439046238225493803";

            Scribd.Net.Search.Criteria crit = new Scribd.Net.Search.Criteria();
            crit.Scope = Scribd.Net.SearchScope.Account;
            crit.MaxResults = pagesize;
            crit.StartIndex = 1;
            //crit.Query = "Svar";

            Scribd.Net.Search.Result res = null;
            do
            {
                res = Scribd.Net.Search.Find(crit);

                //foreach (var doc in res.Documents)
                //{
                //    Scribd.Net.Document.Delete(doc.DocumentId);
                //}

                res.Documents.AsParallel().WithDegreeOfParallelism(10).ForAll(
                    doc => Scribd.Net.Document.Delete(doc.DocumentId)
                );

                Console.WriteLine("deleted " + res.Documents.Count);
                Thread.Sleep(5000);

                crit.StartIndex += pagesize;
            }
            while (res.Documents.Count > 0);

            //Console.WriteLine("found " + res.Documents.Count + " documents");

            //res.Documents.AsParallel().WithDegreeOfParallelism(20).ForAll(
            //        doc => Scribd.Net.Document.Delete(doc.DocumentId)
            //    );
        }

        [TestMethod]
        public void AddAccessKeyToAllScribdDocs()
        {
            int pagesize = 100;

            Scribd.Net.Service.APIKey = "6qoqzj285ftfmvddexcpb";
            Scribd.Net.Service.SecretKey = "sec-6hrkkevcf77mmn34uz73csjmo7";
            Scribd.Net.Service.EnforceSigning = true;
            Scribd.Net.Service.PublisherID = "pub-82439046238225493803";

            //Scribd.Net.Search.Criteria crit = new Scribd.Net.Search.Criteria();
            //crit.Scope = Scribd.Net.SearchScope.Account;
            //crit.MaxResults = pagesize;
            //crit.StartIndex = 1;
            //crit.Query = "Svar";

            var db = new DBDataContext();

            //foreach (var doc in docs)
            //{
            //    var dbdoc = db.Documents.Single(d => d.ScribdId == doc.DocumentId);
            //    dbdoc.ScribdAccessKey = doc.AccessKey;
            //}

            //Scribd.Net.Search.Result res = null;
            List<Scribd.Net.Document> res = null;
            int offset = 0;
            do
            {
                //res = Scribd.Net.Search.Find(crit);
                res = Scribd.Net.Document.GetList(Scribd.Net.Service.User, pagesize, offset, false);

                foreach (var doc in res)
                {
                    var dbdoc = db.Documents.Single(d => d.ScribdId == doc.DocumentId);
                    if (dbdoc.ScribdAccessKey == null)
                    {
                        dbdoc.ScribdAccessKey = doc.AccessKey;
                    }
                }

                //Console.WriteLine("Updated" + res.Documents.Count);
                //Thread.Sleep(5000);

                //crit.StartIndex += pagesize;
                offset += res.Count;
            }
            while (res.Count > 0);

            db.SubmitChanges();
        }
    }
}
