using System.Collections.Generic;
using Advobot.Utilities;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Users
{
	/// <summary>
	/// Checks if the user can be moved from their voice channel.
	/// </summary>
	public class CanBeMovedAttribute : ValidateUserAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<ValidateExtra<SocketGuildUser>> GetExtras()
		{
			yield return ValidationUtils.MovingUserFromVoiceChannel;
		}
	}
}