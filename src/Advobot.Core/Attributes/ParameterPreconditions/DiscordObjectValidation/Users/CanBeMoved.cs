using System;
using System.Collections.Generic;
using Advobot.Utilities;
using Discord;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Users
{
	/// <summary>
	/// Checks if the user can be moved from their voice channel.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class CanBeMovedAttribute : ValidateUserAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<ValidationRule<IGuildUser>> GetValidationRules()
		{
			yield return ValidationUtils.MovingUserFromVoiceChannel;
		}
	}
}