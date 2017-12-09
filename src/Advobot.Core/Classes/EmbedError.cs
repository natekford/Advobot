using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Provides information about why something failed to add to an embed.
	/// </summary>
	public struct EmbedError
	{
		public readonly string Property;
		public readonly string Text;
		public readonly Exception Exception;

		public EmbedError(string property, string text, Exception exception)
		{
			this.Property = property;
			this.Text = text;
			this.Exception = exception;
		}
	}
}
