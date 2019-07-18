using System;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Classes.Attributes.ParameterPreconditions.StringLengthValidation
{
	/// <summary>
	/// Validates the Twitch stream name by making sure it is between 4 and 25 characters and matches a Regex for Twitch usernames.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateTwitchStreamAttribute : ValidateStringLengthAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateTwitchStreamAttribute"/>.
		/// </summary>
		public ValidateTwitchStreamAttribute() : base(4, 25) { }

		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, ParameterInfo parameter, string value, IServiceProvider services)
		{
			var result = await base.CheckPermissionsAsync(context, parameter, value, services).CAF();
			if (!result.IsSuccess)
			{
				return result;
			}
			return RegexUtils.IsValidTwitchName(value)
				? PreconditionResult.FromSuccess()
				: PreconditionResult.FromError("Invalid Twitch username supplied.");
		}
	}
}
