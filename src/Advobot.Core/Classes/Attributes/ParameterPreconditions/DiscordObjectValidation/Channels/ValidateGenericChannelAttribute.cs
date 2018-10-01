using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	/// <summary>
	/// Validates the passed in <see cref="SocketGuildChannel"/>.
	/// </summary>
	public class ValidateGenericChannelAttribute : BaseValidateChannelAttribute
	{
		/// <summary>
		/// Cannot check from context for an unspecified channel type.
		/// </summary>
		public new bool IfNullCheckFromContext => false;

		/// <summary>
		/// Creates an instance of <see cref="ValidateGenericChannelAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public ValidateGenericChannelAttribute(params ChannelPermission[] permissions) : base(permissions) { }

		/// <inheritdoc />
		protected override object GetFromContext(SocketCommandContext context)
			=> throw new NotImplementedException();
	}
}
