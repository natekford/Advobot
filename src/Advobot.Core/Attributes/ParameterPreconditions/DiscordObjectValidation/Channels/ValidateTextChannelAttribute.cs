using Advobot.Modules;
using Discord;
using Discord.WebSocket;
using System;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	/// <summary>
	/// Validates the passed in <see cref="SocketTextChannel"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateTextChannelAttribute : ValidateChannelAttribute
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