using System;
using System.Text;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Provides information about why something failed to add to an embed.
	/// </summary>
	public class EmbedError : Error
	{
		public string Property { get; }
		public string SubProperty { get; }
		public string Value { get; }
		public int RemainingLength { get; private set; }

		internal EmbedError(string property, string subProperty, object value, string reason) : base(reason)
		{
			Property = property;
			SubProperty = subProperty;
			Value = value.ToString();
			RemainingLength = -1;
		}

		internal static EmbedError LengthRemaining(string property, string subProperty, object value, int lengthRemaining)
		{
			return new EmbedError(property, subProperty, value, $"Remaining length is {lengthRemaining}.")
			{
				RemainingLength = lengthRemaining
			};
		}
		internal static EmbedError MaxLength(string property, string subProperty, object value, int maxLength)
		{
			return new EmbedError(property, subProperty, value, $"Max length is {maxLength}.")
			{
				RemainingLength = maxLength
			};
		}
		internal static EmbedError Url(string property, string subProperty, object value)
		{
			return new EmbedError(property, subProperty, value, "Invalid url.");
		}

		public override string ToString()
		{
			var sb = new StringBuilder(Property);
			if (!String.IsNullOrWhiteSpace(SubProperty))
			{
				sb.Append("." + SubProperty);
			}
			sb.Append($": '{Value}' is invalid. Reason: {Reason}");
			return sb.ToString();
		}
	}
}
