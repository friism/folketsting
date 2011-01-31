using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FT.Search
{
    interface ISearchable
    {
        string SolrCollapseId { get; }
        string Title { get; }
        string Subtitle { get; }
        string Summary { get; }
    }
}
