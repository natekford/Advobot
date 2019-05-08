using Advobot.Classes.Modules;
using Discord;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	/// <summary>
	/// Validates the passed in <see cref="SocketCategoryChannel"/>.
	/// </summary>
	public class ValidateCategoryChannelAttribute : ValidateChannelAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateCategoryChannelAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public ValidateCategoryChannelAttribute(params ChannelPermission[] permissions) : base(permissions) { }

		/// <inheritdoc />
		protected override object GetFromContext(AdvobotCommandContext context)
			=> (SocketCategoryChannel)context.Channel.Category;
	}
}