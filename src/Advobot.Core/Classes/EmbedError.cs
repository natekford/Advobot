using Advobot.Core.Interfaces;
using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Provides information about why something failed to add to an embed.
	/// </summary>
	public struct EmbedError : IError
	{
		public string Property { get; }
		public string Text { get; }
		public string Reason { get; }

		public EmbedError(string property, string text, Exception exception)
		{
			Property = property;
			Text = text;
			Reason = exception.Message;
		}
	}
}
