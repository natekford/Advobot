using System;
using System.Threading.Tasks;

using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Users
{
	/// <summary>
	/// Checks if the user can be moved from their voice channel.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class CanBeMovedAttribute : UserParameterPreconditionAttribute
	{
		private static readonly ChannelPermission[] _MoveMembers = new[]
		{
			ChannelPermission.MoveMembers
		};

		/// <inheritdoc />
		public override string Summary
			=> "Can be moved from their current channel";

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckUserAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			IGuildUser user,
			IServiceProvider services)
		{
			if (!(user.VoiceChannel is IVoiceChannel voiceChannel))
			{
				return PreconditionUtils.FromError("The user is not in a voice channel.").Async();
			}
			return invoker.ValidateChannel(voiceChannel, _MoveMembers);
		}
	}
}