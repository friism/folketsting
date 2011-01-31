using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using FT.Model;
using SD = System.Drawing;

namespace FolketsTing.Controllers
{
	public class FileController : Controller
	{
		private readonly IFileRepository _fileRep;

		public FileController()
		{
			_fileRep = new FileRepository();
		}
		
		[OutputCache(Order = 2, Duration = 600, VaryByParam = "imageid")]
		public ActionResult GetImage(string imagename, int imageid)
		{
			FT.DB.Image i = _fileRep.Image(imageid);
			return File(i.Data.ToArray(), i.ContentType, imagename + "." + GetFileType(i.ContentType));
		}

		[OutputCache(Order = 2, Duration = 600, VaryByParam = "imageid;height;width;imagename")]
		public ActionResult GetScaledImage(string imagename, int imageid, int width, int height)
		{
			width = Math.Min(width, 100);
			height = Math.Min(height, 100);

			FT.DB.Image i = _fileRep.Image(imageid);
			SD.Image img = SD.Image.FromStream(new MemoryStream(i.Data.ToArray()));
			img = img.Resize(width, height);

			MemoryStream s = new MemoryStream();
			img.Save(s, GetImageFormat(i.ContentType));
			return File(s.ToArray(), i.ContentType, imagename + "." + GetFileType(i.ContentType));
		}
		
		private static string GetFileType(string contenttype)
		{
			switch (contenttype)
			{
				case "image/jpeg": return "jpg";
				default: throw new ArgumentException(contenttype);
			}
		}

		private static ImageFormat GetImageFormat(string contenttype)
		{
			switch (contenttype)
			{
				case "image/jpeg": return ImageFormat.Jpeg;
				default: throw new ArgumentException(contenttype);
			}
		}
	}

	public static class ImageExtensions
	{
		public static SD.Image Resize(this SD.Image image, int width, int height)
		{
			float scale;
			float scaleWidth = ((float)width / (float)image.Width);
			float scaleHeight = ((float)height / (float)image.Height);
			if (scaleHeight < scaleWidth)
			{
				scale = scaleHeight;
			}
			else
			{
				scale = scaleWidth;
			}

			int destWidth = (int)((image.Width * scale) + 0.5);
			int destHeight = (int)((image.Height * scale) + 0.5);

			SD.Bitmap bitmap = new SD.Bitmap(destWidth, destHeight, PixelFormat.Format24bppRgb);
			bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (SD.Graphics graphics = SD.Graphics.FromImage(bitmap))
			{
				graphics.Clear(SD.Color.White);
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

				graphics.DrawImage(image,
					new SD.Rectangle(0, 0, destWidth, destHeight),
					new SD.Rectangle(0, 0, image.Width, image.Height),
					SD.GraphicsUnit.Pixel);
			}
			return bitmap;
		}
	}
}
