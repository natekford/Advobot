using System;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.StringLengthValidation
{
	/// <summary>
	/// Validates the text channel name by making sure it is between 2 and 100 characters and has no spaces.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateTextChannelNameAttribute : ValidateChannelNameAttribute
	{
		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			string value,
			IServiceProvider services)
		{
			var result = await base.CheckPermissionsAsync(context, parameter, value, services).CAF();
			if (!result.IsSuccess)
			{
				return result;
			}

			if (!value.Contains(" "))
			{
				return PreconditionResult.FromSuccess();
			}
			return PreconditionResult.FromError("Spaces are not allowed in text channel names.");
		}
	}
}
