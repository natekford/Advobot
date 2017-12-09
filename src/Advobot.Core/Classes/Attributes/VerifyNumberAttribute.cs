using Discord.Commands;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Attributes
{
	/// <summary>
	/// Makes sure the passed in number is in the supplied list.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class VerifyNumberAttribute : ParameterPreconditionAttribute
	{
		public readonly ImmutableList<int> ValidNumbers;
		public readonly bool Range;

		public VerifyNumberAttribute(int[] numbers)
		{
			if (numbers.Length > 10)
			{
				throw new ArgumentException($"Don't input more than 10 numbers in a {nameof(VerifyNumberAttribute)}.");
			}

			this.ValidNumbers = numbers.OrderBy(x => x).ToImmutableList();
			this.Range = false;
		}
		public VerifyNumberAttribute(int start, int end)
		{
			this.ValidNumbers = Enumerable.Range(start, end - start).ToImmutableList();
			this.Range = true;
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
			if (value == null)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}

			if (!(value is int num))
			{
				throw new NotSupportedException($"{nameof(VerifyNumberAttribute)} only supports {nameof(UInt32)}.");
			}

			return this.ValidNumbers.Contains(num)
				? Task.FromResult(PreconditionResult.FromSuccess())
				: Task.FromResult(PreconditionResult.FromError($"Invalid {parameter.Name} supplied, must be one of the following: `{String.Join("`, `", this.ValidNumbers)}`"));
		}

		public override string ToString()
		{
			if (!this.Range)
			{
				return $"({String.Join(", ", this.ValidNumbers)})";
			}
			else
			{
				var first = this.ValidNumbers.First();
				var last = this.ValidNumbers.Last();
				return $"({first} to {last})";
			}
		}
	}
}