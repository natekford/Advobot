using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Utilities;
using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites
{
	/// <summary>
	/// Does not allow invites which are not from this guild.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class FromThisGuildAttribute : ValidateInviteAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<ValidationRule<IInviteMetadata>> GetValidationRules()
		{
			yield return (user, target) =>
			{
				if (user.GuildId == target.GuildId)
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
				return Task.FromResult(PreconditionResult.FromError("The passed in invite must belong to this guild."));
			};
		}
	}
}
