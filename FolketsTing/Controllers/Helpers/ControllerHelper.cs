using System.Web;

namespace FolketsTing.Controllers
{

	public static class ControllerHelper
	{
		public static T Or404<T>(this T t)
		{
			if (t != null)
			{
				return t;
			}
			else
			{
				throw new HttpException(404, "Object not found");
			}
		}
	}
}
