using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using Advobot.Utilities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
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
		protected override Task<PreconditionResult> Validate(AdvobotCommandContext context, object value)
			=> context.User.ValidateChannel((IGuildChannel)value, Permissions, GetValidationRules().ToArray());
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