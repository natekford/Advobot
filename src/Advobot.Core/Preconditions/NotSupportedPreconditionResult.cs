
using Discord.Commands;

namespace Advobot.Preconditions
{
	/// <summary>
	/// Result indicating an invalid type was passed in. (Generic attributes would be helpful)
	/// </summary>
	public class NotSupportedPreconditionResult : PreconditionResult
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
		/// Creates an instance of <see cref="NotSupportedPreconditionResult"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="supportedType"></param>
		public NotSupportedPreconditionResult(object value, Type supportedType)
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
}