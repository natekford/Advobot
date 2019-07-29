using System;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using Discord.Commands;

namespace Advobot.Classes.Attributes.ParameterPreconditions.StringLengthValidation
{
	/// <summary>
	/// Certain objects in Discord have minimum and maximum lengths for the names that can be set for them. This attribute verifies those lengths and provides errors stating the min/max if under/over.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class ValidateStringLengthAttribute : AdvobotParameterPreconditionAttribute
	{
		/// <summary>
		/// Minimum valid length for this object.
		/// </summary>
		protected int Min { get; }
		/// <summary>
		/// Maximum valid length for this object.
		/// </summary>
		protected int Max { get; }

		/// <summary>
		/// Creates an instance of <see cref="ValidateStringLengthAttribute"/>.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public ValidateStringLengthAttribute(int min, int max)
		{
			Min = min;
			Max = max;
		}

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			if (!(value is string s))
			{
				throw new NotSupportedException($"{nameof(ValidateStringLengthAttribute)} only supports strings.");
			}
			return CheckPermissionsAsync(context, parameter, s, services);
		}
		/// <summary>
		/// Checks whether the command can execute.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public virtual Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, ParameterInfo parameter, string value, IServiceProvider services)
		{
			if (value.Length < Min)
			{
				return Task.FromResult(PreconditionResult.FromError($"{parameter.Name} must be at least `{Min}` characters long."));
			}
			if (value.Length > Max)
			{
				return Task.FromResult(PreconditionResult.FromError($"{parameter.Name} must be at most `{Max}` characters long."));
			}
			return Task.FromResult(PreconditionResult.FromSuccess());
		}
		/// <summary>
		/// Returns a string saying the min and max characters.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> $"({Min} to {Max} chars)";
	}
}
