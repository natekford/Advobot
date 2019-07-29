using Advobot.Modules;
using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	/// <summary>
	/// Validates the passed in <see cref="SocketVoiceChannel"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateVoiceChannelAttribute : ValidateChannelAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateVoiceChannelAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public ValidateVoiceChannelAttribute(params ChannelPermission[] permissions)
			: base(permissions) { }

		/// <inheritdoc />
		protected override Task<object> GetFromContextAsync(IAdvobotCommandContext context)
			=> Task.FromResult<object>(context.User.VoiceChannel);
	}
}