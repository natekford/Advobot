using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Attributes.Preconditions.Permissions
{
	/// <summary>
	/// For verifying <see cref="SocketGuildUser"/> permissions.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public abstract class RequirePermissionsAttribute
		: PreconditionAttribute, IPrecondition
	{
		/// <summary>
		/// Whether this precondition has to validate the bot's permissions.
		/// </summary>
		public bool AppliesToBot { get; set; }
		/// <summary>
		/// Whether this precondition has to validate the invoker's permissions.
		/// </summary>
		public bool AppliesToInvoker { get; set; } = true;
		/// <summary>
		/// The flags required (each is a separate valid combination of flags).
		/// </summary>
		public ImmutableHashSet<Enum> Permissions { get; }
		/// <inheritdoc />
		public virtual string Summary { get; }

		/// <summary>
		/// Creates an instance of <see cref="RequirePermissionsAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		protected RequirePermissionsAttribute(IEnumerable<Enum> permissions)
		{
			Permissions = permissions.ToImmutableHashSet();
			Summary = Permissions.FormatPermissions();
		}

		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			async Task<PreconditionResult> CheckPermissionsAsync(
				ICommandContext context,
				IGuildUser user,
				IServiceProvider services)
			{
				var perms = await GetUserPermissionsAsync(context, user, services).CAF();
				if (perms == null)
				{
					return PreconditionResult.FromError($"{user.Format()} has no permissions.");
				}
				else if (!Permissions.Any(x => perms.HasFlag(x)))
				{
					return PreconditionResult.FromError($"{user.Format()} does not have any suitable permissions.");
				}
				return this.FromSuccess();
			}

			if (AppliesToInvoker)
			{
				if (context.User is not IGuildUser user)
				{
					return this.FromInvalidInvoker();
				}

				var result = await CheckPermissionsAsync(context, user, services).CAF();
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			if (AppliesToBot)
			{
				var bot = await context.Guild.GetCurrentUserAsync().CAF();

				var result = await CheckPermissionsAsync(context, bot, services).CAF();
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return this.FromSuccess();
		}

		/// <summary>
		/// Returns the invoking user's permissions.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="user"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public abstract Task<Enum?> GetUserPermissionsAsync(
			ICommandContext context,
			IGuildUser user,
			IServiceProvider services);
	}
}