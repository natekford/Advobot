using System;
using System.Linq;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation
{
	/// <summary>
	/// Makes sure the passed in <see cref="ulong"/> is not already banned.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotBannedAttribute : ParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			if (!(value is ulong id))
			{
				throw new NotSupportedException($"{nameof(NotBannedAttribute)} only supports {nameof(Int32)}.");
			}

			var bans = await context.Guild.GetBansAsync().CAF();
			return bans.Select(x => x.User.Id).Contains(id)
				? PreconditionResult.FromError("That user is already banned.")
				: PreconditionResult.FromSuccess();
		}
	}
}
