using System;
using System.Threading.Tasks;

using Advobot.GeneratedParameterPreconditions;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Certain objects in Discord have minimum and maximum lengths for the names that can be set for them. This attribute verifies those lengths and provides errors stating the min/max if under/over.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class StringRangeParameterPreconditionAttribute : StringParameterPreconditionAttribute
	{
		/// <summary>
		/// Allowed length for strings passed in.
		/// </summary>
		public NumberRange<int> Range { get; }
		/// <summary>
		/// The type of string this is targetting.
		/// </summary>
		public abstract string StringType { get; }
		/// <inheritdoc />
		public override string Summary
			=> $"Valid {StringType} ({Range} long)";

		/// <summary>
		/// Creates an instance of <see cref="StringRangeParameterPreconditionAttribute"/>.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		protected StringRangeParameterPreconditionAttribute(int min, int max)
		{
			Range = new NumberRange<int>(min, max);
		}

		/// <inheritdoc />
		protected override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			string value,
			IServiceProvider services)
		{
			if (Range.Contains(value.Length))
			{
				return this.FromSuccess().AsTask();
			}
			return PreconditionResult.FromError($"Invalid {parameter?.Name} supplied, must have a length in `{Range}`").AsTask();
		}
	}
}