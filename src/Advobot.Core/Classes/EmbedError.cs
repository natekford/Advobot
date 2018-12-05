using System.Text;

namespace Advobot.Classes
{
	/// <summary>
	/// Provides information about why something failed to add to an embed.
	/// </summary>
	public sealed class EmbedError
	{
		/// <summary>
		/// The reason for this error.
		/// </summary>
		public string Reason { get; }
		/// <summary>
		/// The main property which had an error. E.G. field.
		/// </summary>
		public string Property { get; }
		/// <summary>
		/// The sub property which had an error. Can be null, or, if Property is field, text/name/length, etc.
		/// </summary>
		public string? SubProperty { get; }
		/// <summary>
		/// The value that gave an error.
		/// </summary>
		public string? Value { get; }
		/// <summary>
		/// The max length remaining. This field is only relevant when the error was length related.
		/// </summary>
		public int RemainingLength { get; }

		internal EmbedError(string property, string? subProperty, object? value, string reason, int remainingLength = -1)
		{
			Reason = reason;
			Property = property;
			SubProperty = subProperty;
			Value = value?.ToString();
			RemainingLength = remainingLength;
		}

		internal static EmbedError LengthRemaining(string property, string? subProperty, object? value, int remainingLength)
			=> new EmbedError(property, subProperty, value, $"Remaining length is {remainingLength}.", remainingLength);
		internal static EmbedError MaxLength(string property, string? subProperty, object? value, int maxLength)
			=> new EmbedError(property, subProperty, value, $"Max length is {maxLength}.", maxLength);
		internal static EmbedError Url(string property, string? subProperty, object? value)
			=> new EmbedError(property, subProperty, value, "Invalid url.");
		/// <summary>
		/// Returns the errors saying the property, sub property, value, and reason.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			var sb = new StringBuilder(Property);
			if (!string.IsNullOrWhiteSpace(SubProperty))
			{
				sb.Append("." + SubProperty);
			}
			sb.Append($": '{Value}' is invalid. Reason: {Reason}");
			return sb.ToString();
		}
	}
}
