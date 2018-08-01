using System;
using Advobot.Interfaces;
using Discord;

namespace Advobot.Classes.ImageResizing
{
	internal struct ImageCreationArguments<T> where T : IImageResizerArguments
	{
		public AdvobotCommandContext Context;
		public T Args;
		public Uri Uri;
		public RequestOptions Options;
		public string NameOrId;
	}
}