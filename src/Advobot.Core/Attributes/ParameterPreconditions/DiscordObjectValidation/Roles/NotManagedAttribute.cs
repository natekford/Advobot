using System;
using System.Collections.Generic;
using Advobot.Utilities;
using Discord;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	/// <summary>
	/// Does not allow managed roles but does allow the everyone role.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotManagedAttribute : RoleAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<Precondition<IRole>> GetPreconditions()
		{
			yield return PreconditionUtils.RoleIsNotManaged;
		}
		/// <inheritdoc />
		public override string ToString()
			=> "Not managed";
	}
}