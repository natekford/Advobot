using System;
using System.Collections.Generic;
using Advobot.Utilities;
using Discord;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites
{
	/// <summary>
	/// Does not allow invites which are not from this guild.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class FromThisGuildAttribute : InviteAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<Precondition<IInviteMetadata>> GetPreconditions()
		{
			yield return (user, target) =>
			{
				if (user.GuildId == target.GuildId)
				{
					return this.FromSuccessAsync();
				}
				return this.FromErrorAsync("The passed in invite must belong to this guild.");
			};
		}
		/// <inheritdoc />
		public override string ToString()
			=> "For this guild";
	}
}
