using Advobot.Classes.Modules;
using Discord;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	/// <summary>
	/// Validates the passed in <see cref="SocketTextChannel"/>.
	/// </summary>
	public class ValidateTextChannelAttribute : BaseValidateChannelAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateTextChannelAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public ValidateTextChannelAttribute(params ChannelPermission[] permissions) : base(permissions) { }

		/// <inheritdoc />
		protected override object GetFromContext(AdvobotCommandContext context)
			=> context.Channel;
	}
}