using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;

namespace FT.DB
{
	public partial class Politician
	{
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name = "Politician_P20Question", Storage = "_P20Questions", ThisKey = "PoliticianId", OtherKey = "AskerPolId")]
		public EntitySet<P20Question> AskerP20Questions {
			get { return this.P20Questions; }
			set { this.P20Questions = value; }
		}

		[global::System.Data.Linq.Mapping.AssociationAttribute(Name = "Politician_P20Question1", Storage = "_P20Questions1", ThisKey = "PoliticianId", OtherKey = "AskeeId")]
		public EntitySet<P20Question> AskeeP20Questions {
			get { return this.P20Questions1; }
			set { this.P20Questions1 = value; }
		}
	}
}
