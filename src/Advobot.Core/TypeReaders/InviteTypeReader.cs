
using Advobot.Attributes;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attempts to find an <see cref="IInviteMetadata"/> on the guild.
	/// </summary>
	[TypeReaderTargetType(typeof(IInviteMetadata))]
	public sealed class InviteTypeReader : TypeReader
	{
		/// <inheritdoc />
		public override async Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			{
				var code = input.Split('/')[^1];
				var invites = await context.Guild.GetInvitesAsync().CAF();
				var invite = invites.FirstOrDefault(x => x.Code.CaseInsEquals(code));
				if (invite != null)
				{
					return TypeReaderResult.FromSuccess(invite);
				}
			}

			{
				var invite = await context.Client.GetInviteAsync(input).CAF();
				if (invite is IInviteMetadata meta && invite.GuildId == context.Guild.Id)
				{
					return TypeReaderResult.FromSuccess(meta);
				}
			}

			return TypeReaderUtils.SingleValidResult(Array.Empty<IInviteMetadata>(), "invites", input);
		}
	}
}