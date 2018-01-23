using System;
using System.Threading.Tasks;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Core.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to get a user id.
	/// </summary>
	public sealed class UserIdTypeReader : TypeReader
	{
		/// <summary>
		/// Checks for a valid user first, then checks for a ulong.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (!ulong.TryParse(input, out var id))
			{
				return TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid user id provided.");
			}
			if (!(await context.Guild.GetUserAsync(id).CAF() is SocketGuildUser user))
			{
				return TypeReaderResult.FromSuccess(id);
			}

			var result = user.Verify(context, new[] { ObjectVerification.CanBeEdited });
			if (!result.IsSuccess)
			{
				return TypeReaderResult.FromSuccess(id);
			}

			return TypeReaderResult.FromError(CommandError.UnmetPrecondition, result.ErrorReason);
		}
	}
}
