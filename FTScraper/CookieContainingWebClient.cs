using System;
using System.Net;

namespace FT.Scraper
{
	public class CookieContainingWebClient : CompressingWebClient
	{
		private System.Net.CookieContainer cookieContainer;
		private string userAgent;
		private int timeout;

		public System.Net.CookieContainer CookieContainer
		{
			get { return cookieContainer; }
			set { cookieContainer = value; }
		}

		public string UserAgent
		{
			get { return userAgent; }
			set { userAgent = value; }
		}

		public int Timeout
		{
			get { return timeout; }
			set { timeout = value; }
		}

		public CookieContainingWebClient()
		{
			timeout = -1;
			cookieContainer = new CookieContainer();
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest request = base.GetWebRequest(address);

			if (request.GetType() == typeof(HttpWebRequest))
			{
				((HttpWebRequest)request).CookieContainer = cookieContainer;
				((HttpWebRequest)request).UserAgent = userAgent;
				((HttpWebRequest)request).Timeout = timeout;
			}

			return request;
		}
	}
}
