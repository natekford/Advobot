using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Certain objects in Discord have minimum and maximum lengths for the names that can be set for them. This attribute verifies those lengths and provides errors stating the min/max if under/over.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class StringParameterPreconditionAttribute
		: AdvobotParameterPreconditionAttribute
	{
		/// <summary>
		/// The type of string this is targetting.
		/// </summary>
		public abstract string StringType { get; }
		/// <inheritdoc />
		public override string Summary
			=> $"Valid {StringType} ({ValidLength} long)";
		/// <inheritdoc />
		public override IEnumerable<Type> SupportedTypes { get; } = new[]
		{
			typeof(string),
		}.ToImmutableArray();
		/// <summary>
		/// Allowed length for strings passed in.
		/// </summary>
		public NumberCollection<int> ValidLength { get; }
		/// <inheritdoc />
		protected override bool AllowEnumerating => false;

		/// <summary>
		/// Creates an instance of <see cref="StringParameterPreconditionAttribute"/>.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		protected StringParameterPreconditionAttribute(int min, int max)
		{
			ValidLength = new NumberCollection<int>(min, max);
		}

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (!(value is string @string))
			{
				return this.FromOnlySupports(value).AsTask();
			}
			return SingularCheckPermissionsAsync(context, parameter, @string, services);
		}

		/// <summary>
		/// Checks whether the condition for the <see cref="string"/> is met before execution of the command.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		protected virtual Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			string value,
			IServiceProvider services)
		{
			if (ValidLength.Contains(value.Length))
			{
				return this.FromSuccess().AsTask();
			}
			return PreconditionResult.FromError($"Invalid {parameter?.Name} supplied, must have a length in `{ValidLength}`").AsTask();
		}
	}
}