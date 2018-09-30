using Advobot.Classes.Results;
using Advobot.Enums;
using Advobot.Utilities;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation
{
	/// <summary>
	/// Validates the passed in <see cref="SocketCategoryChannel"/>.
	/// </summary>
	public class ValidateCategoryChannelAttribute : ValidateDiscordObjectAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateCategoryChannelAttribute"/>.
		/// </summary>
		/// <param name="checks"></param>
		public ValidateCategoryChannelAttribute(params Verif[] checks) : base(checks) { }

		/// <inheritdoc />
		protected override object GetFromContext(SocketCommandContext context)
			=> (SocketCategoryChannel)((SocketTextChannel)context.Channel).Category;
		/// <inheritdoc />
		protected override VerifiedObjectResult? VerifyObject(SocketCommandContext context, object value)
			=> ((SocketCategoryChannel)value).Verify(context, Checks);
	}
}