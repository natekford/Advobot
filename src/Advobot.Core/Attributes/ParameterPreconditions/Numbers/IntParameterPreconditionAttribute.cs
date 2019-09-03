using System;
using System.Threading.Tasks;

using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.Numbers
{
	/// <summary>
	/// Makes sure the passed in number is in the supplied list.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class IntParameterPreconditionAttribute
		: AdvobotParameterPreconditionAttribute
	{
		/// <summary>
		/// Allowed numbers. If the range method is used this will be contain all of the values between the 2.
		/// </summary>
		public NumberCollection<int> Numbers { get; }

		/// <summary>
		/// The type of number this is targetting.
		/// </summary>
		public abstract string NumberType { get; }

		/// <inheritdoc />
		public override string Summary
			=> $"Valid {NumberType} ({Numbers})";

		/// <summary>
		/// Valid numbers which are the randomly supplied values.
		/// </summary>
		/// <param name="numbers"></param>
		protected IntParameterPreconditionAttribute(int[] numbers)
		{
			Numbers = new NumberCollection<int>(numbers);
		}

		/// <summary>
		/// Valid numbers can start at <paramref name="start"/> inclusive or end at <paramref name="end"/> inclusive.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		protected IntParameterPreconditionAttribute(int start, int end)
		{
			Numbers = new NumberCollection<int>(start, end);
		}

		/// <summary>
		/// Checks whether the command can execute.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public virtual Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			int value,
			IServiceProvider services)
		{
			var numbers = GetNumbers(context, parameter, services);
			if (numbers.Contains(value))
			{
				return PreconditionUtils.FromSuccess().AsTask();
			}
			return PreconditionUtils.FromError($"Invalid {parameter?.Name} supplied, must be in `{Numbers}`").AsTask();
		}

		/// <summary>
		/// Returns the number to use for the start.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		protected virtual NumberCollection<int> GetNumbers(
			ICommandContext context,
			ParameterInfo parameter,
			IServiceProvider services)
			=> Numbers;

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (!(value is int num))
			{
				return this.FromOnlySupports(typeof(int)).AsTask();
			}
			return SingularCheckPermissionsAsync(context, parameter, num, services);
		}
	}
}