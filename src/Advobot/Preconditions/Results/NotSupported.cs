using Discord.Commands;

namespace Advobot.Preconditions.Results;

/// <summary>
/// Result indicating an invalid type was passed in. (Generic attributes would be helpful)
/// </summary>
/// <param name="value"></param>
/// <param name="supportedType"></param>
public class NotSupported(object value, Type supportedType) : PreconditionResult(CommandError.ParseFailed, GenerateReason(value, supportedType))
{
	/// <summary>
	/// The types which are supported by the precondition.
	/// </summary>
	public Type SupportedType { get; } = supportedType;
	/// <summary>
	/// The value which is not of the correct type.
	/// </summary>
	public object Value { get; } = value;

	private static string GenerateReason(object value, Type supportedType)
	{
		var type = value.GetType().Name;
		return $"Received object of type `{type}`; only supports `{supportedType.Name}`.";
	}
}