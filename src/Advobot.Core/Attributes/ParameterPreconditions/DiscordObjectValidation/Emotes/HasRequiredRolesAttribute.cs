using System;
using System.Threading.Tasks;

using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Emotes
{
	/// <summary>
	/// Requires the guild emote have roles required to use it.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class HasRequiredRolesAttribute : GuildEmoteParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string Summary => "Has required roles";

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckGuildEmoteAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			GuildEmote emote,
			IServiceProvider services)
		{
			if (emote.RoleIds.Count > 0)
			{
				return PreconditionUtils.FromSuccessAsync();
			}
			return PreconditionUtils.FromErrorAsync("The emote must have required roles.");
		}
	}
}