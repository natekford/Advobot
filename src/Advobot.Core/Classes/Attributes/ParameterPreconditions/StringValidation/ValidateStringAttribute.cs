using System;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using Discord.Commands;

namespace Advobot.Classes.Attributes.ParameterPreconditions.StringValidation
{
	/// <summary>
	/// Certain objects in Discord have minimum and maximum lengths for the names that can be set for them. This attribute verifies those lengths and provides errors stating the min/max if under/over.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class ValidateStringAttribute : AdvobotParameterPreconditionAttribute
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
		/// Creates an instance of <see cref="ValidateStringAttribute"/>.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public ValidateStringAttribute(int min, int max)
		{
			Min = min;
			Max = max;
		}

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			if (!(value is string s))
			{
				throw new NotSupportedException($"{nameof(ValidateStringAttribute)} only supports strings.");
			}
			if (s.Length < Min)
			{
				return Task.FromResult(PreconditionResult.FromError($"{parameter.Name} must be at least `{Min}` characters long."));
			}
			if (s.Length > Max)
			{
				return Task.FromResult(PreconditionResult.FromError($"{parameter.Name} must be at most `{Max}` characters long."));
			}
			if (!AdditionalValidation(s, out var error))
			{
				return Task.FromResult(PreconditionResult.FromError(error));
			}
			return Task.FromResult(PreconditionResult.FromSuccess());
		}
		/// <summary>
		/// Additional validation which is optional to implement.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public virtual bool AdditionalValidation(string s, out string error)
		{
			error = null;
			return true;
		}
		/// <summary>
		/// Returns a string saying the min and max characters.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> $"({Min} to {Max} chars)";
	}
}
