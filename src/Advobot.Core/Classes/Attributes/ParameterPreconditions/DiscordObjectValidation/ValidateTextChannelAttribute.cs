using System;
using Advobot.Classes.Results;
using Advobot.Enums;
using Advobot.Utilities;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation
{
	/// <summary>
	/// Validates the passed in <see cref="SocketTextChannel"/>.
	/// </summary>
	public class ValidateTextChannelAttribute : ValidateDiscordObjectAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateTextChannelAttribute"/>.
		/// </summary>
		/// <param name="checks"></param>
		public ValidateTextChannelAttribute(params Verif[] checks) : base(checks) { }

		/// <inheritdoc />
		protected override object GetFromContext(SocketCommandContext context)
			=> context.Channel;
		/// <inheritdoc />
		protected override VerifiedObjectResult? VerifyObject(SocketCommandContext context, object value)
			=> ((SocketTextChannel)value).Verify(context, Checks);
	}
}