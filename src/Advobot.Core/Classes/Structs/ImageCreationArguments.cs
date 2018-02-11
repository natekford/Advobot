using Advobot.Core.Interfaces;
using Discord;
using System;

namespace Advobot.Core.Classes
{
	public struct ImageCreationArguments<T> where T : IImageResizerArguments
	{
		public Uri Uri;
		public string Name;
		public T Args;
		public AdvobotSocketCommandContext Context;
		public RequestOptions Options;
	}
}
