using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Utilities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	/// <summary>
	/// Base for validating any <see cref="SocketGuildChannel"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class ValidateChannelAttribute : ValidateDiscordObjectAttribute
	{
		/// <summary>
		/// The permissions to make sure the invoking user has on the channel.
		/// </summary>
		public ImmutableHashSet<ChannelPermission> Permissions { get; }
		/// <summary>
		/// Whether this channel can be reordered.
		/// Default value is false.
		/// </summary>
		public bool CanBeReordered { get; set; } = false;

		/// <summary>
		/// Creates an instance of <see cref="ValidateChannelAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public ValidateChannelAttribute(params ChannelPermission[] permissions)
		{
			Permissions = permissions.Concat(new[] { ChannelPermission.ViewChannel }).ToImmutableHashSet();
		}

		/// <inheritdoc />
		protected override Task<PreconditionResult> ValidateAsync(
			ICommandContext context,
			object value)
		{
			if (!(context.User is IGuildUser invoker))
			{
				return Task.FromResult(PreconditionResult.FromError("Invalid invoker."));
			}
			if (!(value is IGuildChannel channel))
			{
				return Task.FromResult(PreconditionResult.FromError("Invalid channel."));
			}
			return invoker.ValidateChannel(channel, Permissions, GetValidationRules().ToArray());
		}
		/// <summary>
		/// Extra checks to use in validation.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<ValidationRule<IGuildChannel>> GetValidationRules()
		{
			if (CanBeReordered)
			{
				yield return ValidationUtils.ChannelCanBeReordered;
			}
		}
	}
}