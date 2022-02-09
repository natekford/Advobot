using Discord.Commands;

namespace Advobot.Preconditions.Results;

/// <summary>
/// Result indicating an invalid type was passed in. (Generic attributes would be helpful)
/// </summary>
public class NotSupported : PreconditionResult
{
	/// <summary>
	/// The types which are supported by the precondition.
	/// </summary>
	public Type SupportedType { get; }
	/// <summary>
	/// The value which is not of the correct type.
	/// </summary>
	public object Value { get; }

	/// <summary>
	/// Creates an instance of <see cref="NotSupported"/>.
	/// </summary>
	/// <param name="value"></param>
	/// <param name="supportedType"></param>
	public NotSupported(object value, Type supportedType)
		: base(CommandError.ParseFailed, GenerateReason(value, supportedType))
	{
		Value = value;
		SupportedType = supportedType;
	}

	private static string GenerateReason(object value, Type supportedType)
	{
		var type = value.GetType().Name;
		return $"Received object of type `{type}`; only supports `{supportedType.Name}`.";
	}
}