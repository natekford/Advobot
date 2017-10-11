using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

		public VerifyNumberAttribute(params int[] numbers)
		{
			if (numbers.Length > 10)
			{
				throw new ArgumentException($"Don't input more than 10 numbers in a {nameof(VerifyNumberAttribute)}.");
			}

			ValidNumbers = numbers.OrderBy(x => x).ToImmutableList();
		}
		public VerifyNumberAttribute(bool soDiffOverloads, int start, int end)
		{
			if (!soDiffOverloads)
			{
				throw new ArgumentException($"Make the bool in a {nameof(VerifyNumberAttribute)} true instead of false.");
			}

			ValidNumbers = Enumerable.Range(start, end - start).ToImmutableList();
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
			if (ValidNumbers.Count <= 10)
			{
				return $"({String.Join(", ", ValidNumbers)})";
			}
			else
			{
				var first = ValidNumbers.First();
				var last = ValidNumbers.Last();
				return $"({first} to {last})";
			}
		}
	}
}