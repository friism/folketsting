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

using System;

namespace FT.Search.Helpers {
    public static class StringExtensions {
        public static bool NotNullAnd(this string s, Func<string, bool> f) {
            return s != null && f(s);
        }

        public static string FacetInflector(this string s)
        {
            string retVal = s;
            switch (s)
            {
                case "PartyName":
                    retVal = "Parti";
                    break;
                case "DocTypeHumanized":
                    retVal = "Type";
                    break;
                case "PoliticianName":
                    retVal = "Politiker";
                    break;
                case "MinistryName":
                    retVal = "Ministerie";
                    break;
                case "StageHumanized":
                    retVal = "Behandling";
                    break;
                case "ProposedYear":
                    retVal = "År";
                    break;
            }
            return retVal;
        }
    }
}