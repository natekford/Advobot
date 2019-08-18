using System;
using System.Collections.Generic;
using System.Linq;
using Advobot.Utilities;
using Discord;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Emotes
{
	/// <summary>
	/// Requires the guild emote have roles required to use it.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class HasRequiredRolesAttribute : GuildEmoteAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<Precondition<GuildEmote>> GetPreconditions()
		{
			yield return (user, target) =>
			{
				if (target.RoleIds.Any())
				{
					return this.FromSuccessAsync();
				}
				return this.FromErrorAsync("The passed in emote must have required roles.");
			};
		}
		/// <inheritdoc />
		public override string ToString()
			=> "Has required roles";
	}
}
