using System;
using System.Collections.Generic;
using Advobot.Utilities;
using Discord;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	/// <summary>
	/// Does not allow the everyone role or managed roles.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotEveryoneOrManagedAttribute : RoleAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<Precondition<IRole>> GetPreconditions()
		{
			yield return PreconditionUtils.RoleIsNotEveryone;
			yield return PreconditionUtils.RoleIsNotManaged;
		}
		/// <inheritdoc />
		public override string ToString()
			=> "Not everyone or managed";
	}
}