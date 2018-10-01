using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	/// <summary>
	/// Validates the passed in <see cref="SocketVoiceChannel"/>.
	/// </summary>
	public class ValidateVoiceChannelAttribute : BaseValidateChannelAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateVoiceChannelAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public ValidateVoiceChannelAttribute(params ChannelPermission[] permissions) : base(permissions) { }

		/// <inheritdoc />
		protected override object GetFromContext(SocketCommandContext context)
			=> ((SocketGuildUser)context.User).VoiceChannel;
	}
}