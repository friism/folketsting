using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FT.Scraper.Tests
{
    class Runner
    {
        public static void Main(string[] args)
        {
            var scribdrunner = new ScribdTest();
            scribdrunner.AddAccessKeyToAllScribdDocs();
        }
    }
}
