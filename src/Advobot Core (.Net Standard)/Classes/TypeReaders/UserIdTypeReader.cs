using Advobot.Actions;
using Advobot.Enums;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.Classes.TypeReaders
{
	public class UserIdTypeReader : TypeReader
	{
		/// <summary>
		/// Checks for a valid user first, then checks for a ulong.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			if (!ulong.TryParse(input, out ulong id))
			{
				return TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid user id provided.");
			}

			var user = await context.Guild.GetUserAsync(id);
			if (user == null)
			{
				return TypeReaderResult.FromSuccess(id);
			}

			var result = UserActions.VerifyUserMeetsRequirements(context, user, new[] { ObjectVerification.CanBeEdited });
			if (!result.IsSuccess)
			{
				return TypeReaderResult.FromSuccess(id);
			}

			return TypeReaderResult.FromError(CommandError.UnmetPrecondition, result.ErrorReason);
		}
	}
}
