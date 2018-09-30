using System;
using Advobot.Classes.Results;
using Advobot.Enums;
using Advobot.Utilities;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation
{
	/// <summary>
	/// Validates the passed in <see cref="SocketRole"/>.
	/// </summary>
	public class ValidateRoleAttribute : ValidateDiscordObjectAttribute
	{
		/// <summary>
		/// Cannot check from context for roles.
		/// </summary>
		public new bool IfNullCheckFromContext => false;

		/// <summary>
		/// Creates an instance of <see cref="ValidateRoleAttribute"/>.
		/// </summary>
		/// <param name="checks"></param>
		public ValidateRoleAttribute(params Verif[] checks) : base(checks) { }

		/// <inheritdoc />
		protected override object GetFromContext(SocketCommandContext context)
			=> throw new NotImplementedException();
		/// <inheritdoc />
		protected override VerifiedObjectResult? VerifyObject(SocketCommandContext context, object value)
			=> ((SocketRole)value).Verify(context, Checks);
	}
}