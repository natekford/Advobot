using Advobot.Classes.Results;
using Advobot.Enums;
using Advobot.Utilities;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation
{
	/// <summary>
	/// Validates the passed in <see cref="SocketVoiceChannel"/>.
	/// </summary>
	public class ValidateVoiceChannelAttribute : ValidateDiscordObjectAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateVoiceChannelAttribute"/>.
		/// </summary>
		/// <param name="checks"></param>
		public ValidateVoiceChannelAttribute(params Verif[] checks) : base(checks) { }

		/// <inheritdoc />
		protected override object GetFromContext(SocketCommandContext context)
			=> ((SocketGuildUser)context.User).VoiceChannel;
		/// <inheritdoc />
		protected override VerifiedObjectResult? VerifyObject(SocketCommandContext context, object value)
			=> ((SocketVoiceChannel)value).Verify(context, Checks);
	}
}