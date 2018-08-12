using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Makes sure the passed in number is in the supplied list.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class VerifyNumberAttribute : ParameterPreconditionAttribute
	{
		/// <summary>
		/// Allowed numbers. If the range method is used this will be empty.
		/// </summary>
		public ImmutableList<int> ValidNumbers { get; }
		/// <summary>
		/// The starting value.
		/// </summary>
		public int Start { get; }
		/// <summary>
		/// The ending value.
		/// </summary>
		public int End { get; }

		/// <summary>
		/// Valid numbers which are the randomly supplied values.
		/// </summary>
		/// <param name="numbers"></param>
		public VerifyNumberAttribute(int[] numbers)
		{
			if (numbers.Length > 50)
			{
				throw new ArgumentException("Don't input more than 50 numbers.", nameof(numbers));
			}

			ValidNumbers = numbers.OrderBy(x => x).ToImmutableList();
			Start = int.MaxValue;
			End = int.MinValue;
		}
		/// <summary>
		/// Valid numbers can start at <paramref name="start"/> inclusive or end at <paramref name="end"/> inclusive.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public VerifyNumberAttribute(int start, int end)
		{
			ValidNumbers = new int[0].ToImmutableList();
			Start = start;
			End = end;
		}

		/// <summary>
		/// Makes sure the supplied value is a valid number.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
			if (value == null)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			if (!int.TryParse(value.ToString(), out var num))
			{
				throw new NotSupportedException($"{nameof(VerifyNumberAttribute)} only supports {nameof(Int32)}.");
			}
			if (ValidNumbers.Any())
			{
				return ValidNumbers.Contains(num)
					? Task.FromResult(PreconditionResult.FromSuccess())
					: Task.FromResult(PreconditionResult.FromError($"Invalid {parameter.Name} supplied, must be one of the following: `{String.Join("`, `", ValidNumbers)}`"));
			}
			return num >= Start && num <= End
				? Task.FromResult(PreconditionResult.FromSuccess())
				: Task.FromResult(PreconditionResult.FromError($"Invalid {parameter.Name} supplied, must be between `{Start}` and `{End}`."));
		}

		/// <summary>
		/// Returns a string indicating what the valid numbers are.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return ValidNumbers.Any() ? $"({String.Join(", ", ValidNumbers)}" : $"({Start} to {End})";
		}
	}
}