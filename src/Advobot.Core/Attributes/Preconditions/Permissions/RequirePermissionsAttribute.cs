﻿using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Attributes.Preconditions.Permissions
{
	/// <summary>
	/// For verifying <see cref="SocketGuildUser"/> permissions.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public abstract class RequirePermissionsAttribute : PreconditionAttribute
	{
		/// <summary>
		/// The flags required (each is a separate valid combination of flags).
		/// </summary>
		public ImmutableHashSet<Enum> Permissions { get; }

		private readonly string _PermissionsText;

		/// <summary>
		/// Creates an instance of <see cref="RequirePermissionsAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public RequirePermissionsAttribute(params Enum[] permissions)
		{
			Permissions = permissions.ToImmutableHashSet();

			_PermissionsText = Permissions.FormatPermissions();
		}

		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			var userPerms = await GetUserPermissionsAsync(context, services).CAF();
			//If the user has no permissions this should just return an error
			if (userPerms == null)
			{
				return this.FromError("You have no permissions.");
			}

			foreach (var flag in Permissions)
			{
				if (userPerms.HasFlag(flag))
				{
					return this.FromSuccess();
				}
			}
			return this.FromError("You are missing permissions.");
		}
		/// <summary>
		/// Returns the invoking user's permissions.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public abstract Task<Enum?> GetUserPermissionsAsync(
			ICommandContext context,
			IServiceProvider services);
		/// <inheritdoc />
		public override string ToString()
			=> _PermissionsText;
	}
}