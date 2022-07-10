namespace Advobot.Embeds;

/// <summary>
/// An error which occurs when attempting to modify an <see cref="EmbedWrapper"/>.
/// </summary>
public class EmbedException : Exception
{
	/// <summary>
	/// The property which had an error.
	/// </summary>
	public string PropertyPath { get; } = "";
	/// <summary>
	/// The value that gave an error.
	/// </summary>
	public object? Value { get; }

	/// <inheritdoc cref="EmbedException(string, string, object?, Exception?)" />
	public EmbedException()
		: base()
	{
	}

	/// <inheritdoc cref="EmbedException(string, string, object?, Exception?)" />
	public EmbedException(string message)
		: base(message)
	{
	}

	/// <inheritdoc cref="EmbedException(string, string, object?, Exception?)" />
	public EmbedException(string message, Exception? innerException)
		: base(message, innerException)
	{
	}

	/// <inheritdoc cref="EmbedException(string, string, object?, Exception?)" />
	public EmbedException(string message, string propertyPath, object? value)
		: this(message, propertyPath, value, null)
	{
	}

	/// <summary>
	/// Creates an instance of <see cref="EmbedException"/>.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="propertyPath"></param>
	/// <param name="value"></param>
	/// <param name="innerException"></param>
	public EmbedException(
		string message,
		string propertyPath,
		object? value,
		Exception? innerException)
		: this(
			message: $"{propertyPath}: '{value?.ToString() ?? "null"}' is invalid. Reason: {message}",
			innerException: innerException
		)
	{
		PropertyPath = propertyPath;
		Value = value;
	}
}