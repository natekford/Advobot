using System;
using Advobot.Classes.Results;
using Advobot.Enums;
using Advobot.Utilities;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation
{
	/// <summary>
	/// Validates the passed in <see cref="SocketGuildChannel"/>.
	/// </summary>
	public class ValidateGenericChannelAttribute : ValidateDiscordObjectAttribute
	{
		/// <summary>
		/// Cannot check from context for an unspecified channel type.
		/// </summary>
		public new bool IfNullCheckFromContext => false;

		/// <summary>
		/// Creates an instance of <see cref="ValidateGenericChannelAttribute"/>.
		/// </summary>
		/// <param name="checks"></param>
		public ValidateGenericChannelAttribute(params Verif[] checks) : base(checks) { }

		/// <inheritdoc />
		protected override object GetFromContext(SocketCommandContext context)
			=> throw new NotImplementedException();
		/// <inheritdoc />
		protected override VerifiedObjectResult? VerifyObject(SocketCommandContext context, object value)
			=> ((SocketGuildChannel)value).Verify(context, Checks);
	}
}
