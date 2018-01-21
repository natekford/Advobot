﻿using Discord.Commands;
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
		public ImmutableArray<int> ValidNumbers { get; }
		public int Start { get; }
		public int End { get; }

		public VerifyNumberAttribute(int[] numbers)
		{
			if (numbers.Length > 10)
			{
				throw new ArgumentException("don't input more than 10 numbers", nameof(numbers));
			}

			ValidNumbers = numbers.OrderBy(x => x).ToImmutableArray();
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
			ValidNumbers = new int[0].ToImmutableArray();
			Start = start;
			End = end;
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
			if (value == null)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			else if (!int.TryParse(value.ToString(), out var num))
			{
				throw new NotSupportedException($"{nameof(VerifyNumberAttribute)} only supports {nameof(Int32)}.");
			}
			else if (ValidNumbers.Any())
			{
				return ValidNumbers.Contains((int)num)
					? Task.FromResult(PreconditionResult.FromSuccess())
					: Task.FromResult(PreconditionResult.FromError($"Invalid {parameter.Name} supplied, must be one of the following: `{String.Join("`, `", ValidNumbers)}`"));
			}
			else
			{
				return num >= Start && num <= End
					? Task.FromResult(PreconditionResult.FromSuccess())
					: Task.FromResult(PreconditionResult.FromError($"Invalid {parameter.Name} supplied, must be between `{Start}` and `{End}`."));
			}				
		}

		public override string ToString()
		{
			if (!ValidNumbers.Any())
			{
				return $"({Start} to {End})";
			}
			else
			{
				return $"({String.Join(", ", ValidNumbers)}";
			}
		}
	}
}