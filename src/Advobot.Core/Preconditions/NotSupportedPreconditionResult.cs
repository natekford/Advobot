using System;
using System.Collections.Generic;
using System.Linq;

using AdvorangesUtils;

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
		public IEnumerable<Type> SupportedTypes { get; }
		/// <summary>
		/// The value which is not of the correct type.
		/// </summary>
		public object Value { get; }

		/// <summary>
		/// Creates an instance of <see cref="NotSupportedPreconditionResult"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="supportedTypes"></param>
		public NotSupportedPreconditionResult(object value, IEnumerable<Type> supportedTypes)
			: base(CommandError.ParseFailed, GenerateReason(value, supportedTypes))
		{
			Value = value;
			SupportedTypes = supportedTypes;
		}

		private static string GenerateReason(object value, IEnumerable<Type> supportedTypes)
		{
			var type = value.GetType().Name;
			var supported = supportedTypes.Join(x => x.Name);
			return $"Received object of type {type}; only supports {supported}.";
		}
	}
}