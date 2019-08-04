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
	public class CanBeMovedAttribute : UserAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<Precondition<IGuildUser>> GetPreconditions()
		{
			yield return PreconditionUtils.MovingUserFromVoiceChannel;
		}
		/// <inheritdoc />
		public override string ToString()
			=> "Can be moved from their current channel";
	}
}