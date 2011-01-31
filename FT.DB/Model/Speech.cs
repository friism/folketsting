using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FT.DB
{
	public partial class Speech
	{
		public Speech ParentSpeech {
			get { return this.Speech1; }
			set { this.Speech1 = value; }
		}
	}
}
