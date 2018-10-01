using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	/// <summary>
	/// Validates the passed in <see cref="SocketCategoryChannel"/>.
	/// </summary>
	public class ValidateCategoryChannelAttribute : BaseValidateChannelAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateCategoryChannelAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public ValidateCategoryChannelAttribute(params ChannelPermission[] permissions) : base(permissions) { }

		/// <inheritdoc />
		protected override object GetFromContext(SocketCommandContext context)
			=> (SocketCategoryChannel)((SocketTextChannel)context.Channel).Category;
	}
}