using System;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.StringLengthValidation
{
	/// <summary>
	/// Validates the rule category by making sure it is between 1 and 250 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateRuleCategoryAttribute : ValidateStringLengthAttribute
	{
		/// <summary>
		/// If true, returns an error if the category already exists. Otherwise, returns an error if the category does not exist.
		/// </summary>
		public bool ErrorOnCategoryExisting { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="ValidateRuleCategoryAttribute"/>.
		/// </summary>
		public ValidateRuleCategoryAttribute() : base(1, 250) { }

		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, ParameterInfo parameter, string value, IServiceProvider services)
		{
			var result = await base.CheckPermissionsAsync(context, parameter, value, services).CAF();
			if (!result.IsSuccess)
			{
				return result;
			}
			var categoryExists = context.GuildSettings.Rules?.Categories?.Keys?.CaseInsContains(value) ?? false;
			if (ErrorOnCategoryExisting && categoryExists)
			{
				return PreconditionResult.FromError($"`{value}` already exists as a rule category.");
			}
			if (!ErrorOnCategoryExisting && !categoryExists)
			{
				return PreconditionResult.FromError($"`{value}` is not a command category.");
			}
			return PreconditionResult.FromSuccess();
		}
	}
}
