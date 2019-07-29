using Advobot.Classes.Modules;
using AdvorangesUtils;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.Attributes.ParameterPreconditions.StringLengthValidation
{
	/// <summary>
	/// Validates the text channel name by making sure it is between 2 and 100 characters and has no spaces.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateTextChannelNameAttribute : ValidateChannelNameAttribute
	{
		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, ParameterInfo parameter, string value, IServiceProvider services)
		{
			var result = await base.CheckPermissionsAsync(context, parameter, value, services).CAF();
			if (!result.IsSuccess)
			{
				return result;
			}
			return value.Contains(" ")
				? PreconditionResult.FromError("Spaces are not allowed in text channel names.")
				: PreconditionResult.FromSuccess();
		}
	}
}
