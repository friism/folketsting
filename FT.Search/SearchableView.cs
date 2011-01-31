#region license
// Copyright (c) 2007-2009 Mauricio Scheffer
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Collections.Generic;
using SolrNet.Commands.Parameters;
using SolrNet;

namespace FT.Search {
    public class SearchableView
    {
        public SearchParameters Search { get; set; }
        public ICollection<Searchable> Searchables { get; set; }
        public PaginationInfo PaginationInfo { get; set; }
        public int TotalCount { get; set; }
        public IDictionary<string, ICollection<KeyValuePair<string, int>>> Facets { get; set; }
        public string DidYouMean { get; set; }
        public bool QueryError { get; set; }
        public IDictionary<Searchable, IDictionary<string, ICollection<string>>> Highlights { get; set; }
        public int QueryTime { get; set; }
        public IEnumerable<SuperSearchable> SuperSearchables { get; set; }
        public CollapseResults CollapseResults { get; set; }

        public SearchableView() {
            Search = new SearchParameters();
            Facets = new Dictionary<string, ICollection<KeyValuePair<string, int>>>();
            Searchables = new List<Searchable>();
        }
    }
}