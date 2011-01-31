using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FT.Search;

namespace FT.Search.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class SearchableUnittest
    {
        private Searchable _searchable;
        public SearchableUnittest()
        {
            _searchable = new Searchable();
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void ShouldGetAndSetIdProperty()
        {
            _searchable.LawId = 123;
            Assert.AreEqual(123, _searchable.LawId);
        }

        [TestMethod]
        public void ShouldGetAndSetTitleProperty()
        {
            string _testString = "Test Searchable";
            _searchable.Title = _testString;
            Assert.AreEqual(_testString, _searchable.Title);
        }

        [TestMethod]
        public void ShouldGetAndSetDoctype()
        {
            string _testString = "Searchable";
            _searchable.DocType = _testString;
            Assert.AreEqual(_testString, _searchable.DocType);
        }
    }
}
