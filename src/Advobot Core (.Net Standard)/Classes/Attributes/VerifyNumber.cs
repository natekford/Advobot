using Discord.Commands;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Makes sure the passed in number is in the supplied list.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	internal sealed class VerifyNumberAttribute : ParameterPreconditionAttribute
	{
		public readonly ImmutableList<int> ValidNumbers;

		/// <summary>
		/// Sets the valid numbers.
		/// </summary>
		/// <param name="numbers"></param>
		public VerifyNumberAttribute(params int[] numbers)
		{
			ValidNumbers = numbers.ToImmutableList();
		}

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
			if (value == null)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}

			var nullableNum = value as int?;
			if (nullableNum == null)
			{
				throw new NotSupportedException($"{nameof(VerifyNumberAttribute)} only supports {nameof(UInt32)}.");
			}

			var num = nullableNum.Value;
			return ValidNumbers.Contains(num)
				? Task.FromResult(PreconditionResult.FromSuccess())
				: Task.FromResult(PreconditionResult.FromError($"Invalid {parameter.Name} supplied, must be one of the following: `{String.Join("`, `", ValidNumbers)}`"));
		}

		public override string ToString()
		{
			return $"({String.Join(", ", ValidNumbers)})";
		}
	}
}