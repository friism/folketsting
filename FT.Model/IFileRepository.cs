using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FT.DB;
using System.Web.Mvc;

namespace FT.Model
{
	public interface IFileRepository
	{
		Image Image(int imageid);
	}

	public class FileRepository : Repository, IFileRepository
	{
		public Image Image(int imageid)
		{
			return DB.Images.Single(i => i.ImageId == imageid);
		}
	}
}
