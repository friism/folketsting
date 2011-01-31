
namespace System.Web.Mvc
{
	public static class HtmlHelperExtensions
	{
		public static string RpxLoginEmbedded(this HtmlHelper helper, 
			string applicationName, string tokenUrl)
		{
			return "<iframe src=\"https://" + applicationName + 
				".rpxnow.com/openid/embed?token_url=" + tokenUrl + 
				"\" scrolling=\"no\" frameBorder=\"no\" " + 
				"style=\"width:400px;height:240px;\" class=\"rpx-embedded\"></iframe>";
		}

		public static string RpxLoginPopup(this HtmlHelper helper, 
			string applicationName, string tokenUrl, string linkText)
		{
			return "<script src=\"https://rpxnow.com/openid/v2/widget\" " + 
				" type=\"text/javascript\"></script> " + 
				"<script type=\"text/javascript\">RPXNOW.overlay = true; RPXNOW.language_preference = 'en';</script>" +
				"<a class=\"rpxnow\" onclick=\"return false;\" href=\"https://" + 
				applicationName + ".rpxnow.com/openid/v2/signin?token_url=" + 
				tokenUrl + "\">" + linkText + "</a>";
		}
	}
}
