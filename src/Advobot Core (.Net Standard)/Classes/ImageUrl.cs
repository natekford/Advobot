using System;

namespace Advobot.Classes
{
	public class ImageUrl
	{
		public Uri Url { get; }
		public string FileType { get; }

		public ImageUrl(string url, string fileType)
		{
			Url = new Uri(url);
			FileType = fileType;
		}
	}
}
