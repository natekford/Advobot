using System;
using System.Collections.Generic;
using Advobot.Utilities;
using Discord;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	/// <summary>
	/// Does not allow roles which are already mentionable to be used.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotMentionableAttribute : RoleAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<Precondition<IRole>> GetPreconditions()
		{
			yield return PreconditionUtils.RoleIsNotMentionable;
		}
		/// <inheritdoc />
		public override string ToString()
			=> "Not already mentionable";
	}
}