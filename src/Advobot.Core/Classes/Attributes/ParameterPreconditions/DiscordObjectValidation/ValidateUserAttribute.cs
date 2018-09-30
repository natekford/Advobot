using Advobot.Classes.Results;
using Advobot.Enums;
using Advobot.Utilities;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation
{
	/// <summary>
	/// Validates the passed in <see cref="SocketGuildUser"/>.
	/// </summary>
	public class ValidateUserAttribute : ValidateDiscordObjectAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateUserAttribute"/>.
		/// </summary>
		/// <param name="checks"></param>
		public ValidateUserAttribute(params Verif[] checks) : base(checks) { }

		/// <inheritdoc />
		protected override object GetFromContext(SocketCommandContext context)
			=> (SocketGuildUser)context.User;
		/// <inheritdoc />
		protected override VerifiedObjectResult? VerifyObject(SocketCommandContext context, object value)
			=> ((SocketGuildUser)value).Verify(context, Checks);
	}
}