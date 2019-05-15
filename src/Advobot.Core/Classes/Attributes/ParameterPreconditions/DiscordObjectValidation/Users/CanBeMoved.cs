﻿using System;
using System.Collections.Generic;
using Advobot.Utilities;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Users
{
	/// <summary>
	/// Checks if the user can be moved from their voice channel.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class CanBeMovedAttribute : ValidateUserAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<ValidationRule<SocketGuildUser>> GetValidationRules()
		{
			yield return ValidationUtils.MovingUserFromVoiceChannel;
		}
	}
}