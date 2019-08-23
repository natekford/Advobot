using System;
using System.Collections.Generic;
using Advobot.Utilities;
using Discord;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites
{
	/// <summary>
	/// Does not allow invites which can expire.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NeverExpiresAttribute : InviteAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<Precondition<IInviteMetadata>> GetPreconditions()
		{
			yield return (user, target) =>
			{
				if (target.MaxAge == null)
				{
					return PreconditionUtils.FromSuccessAsync();
				}
				return PreconditionUtils.FromErrorAsync("The passed in invite must not expire.");
			};
		}
		/// <inheritdoc />
		public override string ToString()
			=> "Never expires";
	}
}
