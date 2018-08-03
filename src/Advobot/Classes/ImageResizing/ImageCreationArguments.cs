using System;
using Advobot.Interfaces;
using Discord;

namespace Advobot.Classes.ImageResizing
{
	internal sealed class ImageCreationArguments<T> where T : IImageResizerArguments
	{
		public AdvobotCommandContext Context { get; set; }
		public T Args { get; set; }
		public Uri Uri { get; set; }
		public RequestOptions Options { get; set; }
		public string NameOrId { get; set; }
	}
}