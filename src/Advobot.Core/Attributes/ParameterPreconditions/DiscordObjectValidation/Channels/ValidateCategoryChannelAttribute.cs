using System;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	/// <summary>
	/// Validates the passed in <see cref="SocketCategoryChannel"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateCategoryChannelAttribute : ValidateChannelAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateCategoryChannelAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public ValidateCategoryChannelAttribute(params ChannelPermission[] permissions)
			: base(permissions) { }

		/// <inheritdoc />
		protected override async Task<object> GetFromContextAsync(ICommandContext context)
		{
			if (!(context.Channel is INestedChannel channel))
			{
				throw new ArgumentException("Invalid channel.");
			}
			return await channel.GetCategoryAsync().CAF();
		}
	}
}