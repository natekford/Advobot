using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

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
		protected override Task<object> GetFromContextAsync(ICommandContext context)
		{
			if (!(context.User is IGuildUser user))
			{
				throw new ArgumentException("Invalid user.");
			}
			return Task.FromResult<object>(user.VoiceChannel);
		}
	}
}