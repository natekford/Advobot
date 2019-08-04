using System;
using System.Collections.Generic;
using Advobot.Utilities;
using Discord;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	/// <summary>
	/// Does not allow the everyone role but does allow managed roles.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotEveryoneAttribute : RoleAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<Precondition<IRole>> GetPreconditions()
		{
			yield return PreconditionUtils.RoleIsNotEveryone;
		}
		/// <inheritdoc />
		public override string ToString()
			=> "Not everyone";
	}
}