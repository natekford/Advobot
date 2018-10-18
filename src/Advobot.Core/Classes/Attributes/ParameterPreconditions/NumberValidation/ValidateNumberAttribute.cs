using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation
{
	/// <summary>
	/// Makes sure the passed in number is in the supplied list.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class ValidateNumberAttribute : AdvobotParameterPreconditionAttribute
	{
		/// <summary>
		/// Allowed numbers. If the range method is used this will be empty.
		/// </summary>
		public ImmutableArray<int> ValidNumbers { get; }
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
		public ValidateNumberAttribute(int[] numbers)
		{
			ValidNumbers = numbers.OrderBy(x => x).ToImmutableArray();
			Start = int.MinValue;
			End = int.MaxValue;
		}
		/// <summary>
		/// Valid numbers can start at <paramref name="start"/> inclusive or end at <paramref name="end"/> inclusive.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public ValidateNumberAttribute(int start, int end)
		{
			ValidNumbers = new int[0].ToImmutableArray();
			Start = start;
			End = end;
		}

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			if (!(value is int num) || !int.TryParse(value.ToString(), out num))
			{
				throw new NotSupportedException($"{nameof(ValidateNumberAttribute)} only supports {nameof(Int32)}.");
			}
			if (ValidNumbers.Any())
			{
				return ValidNumbers.Contains(num)
					? Task.FromResult(PreconditionResult.FromSuccess())
					: Task.FromResult(PreconditionResult.FromError($"Invalid {parameter.Name} supplied, must be one of the following: `{string.Join("`, `", ValidNumbers)}`"));
			}
			var start = GetStart(context);
			var end = GetEnd(context);
			return num >= start && num <= end
				? Task.FromResult(PreconditionResult.FromSuccess())
				: Task.FromResult(PreconditionResult.FromError($"Invalid {parameter.Name} supplied, must be between `{start}` and `{end}`."));
		}
		/// <summary>
		/// Returns the number to use for the start. This will only be used if <see cref="ValidNumbers"/> is empty.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public virtual int GetStart(ICommandContext context)
			=> Start;
		/// <summary>
		/// Returns the number to use for the end. This will only be used if <see cref="ValidNumbers"/> is empty.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public virtual int GetEnd(ICommandContext context)
			=> End;
		/// <summary>
		/// Returns a string indicating what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> ValidNumbers.Any() ? $"({string.Join(", ", ValidNumbers)})" : $"({Start} to {End})";
	}
}