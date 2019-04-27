using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Advobot.Classes.Modules;
using Advobot.Classes.Results;
using Advobot.Utilities;
using Discord;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	/// <summary>
	/// Base for validating any <see cref="SocketGuildChannel"/>.
	/// </summary>
	public abstract class BaseValidateChannelAttribute : ValidateDiscordObjectAttribute
	{
		/// <summary>
		/// The permissions to make sure the invoking user has on the channel.
		/// </summary>
		public ImmutableArray<ChannelPermission> Permissions { get; }
		/// <summary>
		/// Whether this channel can be reordered.
		/// Default value is false.
		/// </summary>
		public bool CanBeReordered { get; set; } = false;

		/// <summary>
		/// Creates an instance of <see cref="BaseValidateChannelAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public BaseValidateChannelAttribute(params ChannelPermission[] permissions)
		{
			if (!permissions.Contains(ChannelPermission.ViewChannel))
			{
				Array.Resize(ref permissions, permissions.Length + 1);
				permissions[permissions.Length - 1] = ChannelPermission.ViewChannel;
			}
			Permissions = permissions.OrderBy(x => x).ToImmutableArray();
		}

		/// <inheritdoc />
		protected override VerifiedObjectResult ValidateObject(AdvobotCommandContext context, object value)
			=> context.User.ValidateChannel((SocketGuildChannel)value, Permissions, GetExtras().ToArray());
		/// <summary>
		/// Extra checks to use in validation.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<ValidationRule<SocketGuildChannel>> GetExtras()
		{
			if (CanBeReordered)
			{
				yield return ValidationUtils.ChannelCanBeReordered;
			}
		}
	}
}