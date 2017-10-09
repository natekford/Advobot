namespace Advobot.Classes
{
	public class ImageUrl
	{
		public string Url { get; }
		public string FileType { get; }

		public ImageUrl(string url, string fileType)
		{
			Url = url;
			FileType = fileType;
		}
	}
}
