using System;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha.TypeReaders
{
	[TypeReaderTargetType(typeof(User))]
	public sealed class UserTypeReader : UserTypeReader<IUser>
	{
		public bool CreateIfNotFound { get; set; }

		//TODO: add in the ability to get users who have left the server
		public override async Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			var result = await base.ReadAsync(context, input, services).CAF();
			if (!result.IsSuccess) //Can't find a discord user with the supplied string
			{
				return result;
			}

			var user = (IUser)result.BestMatch;
			var db = services.GetRequiredService<IGachaDatabase>();
			var entry = await db.GetUserAsync(context.Guild.Id, user.Id).CAF();
			if (entry != null) //Profile already exists, can return that
			{
				return TypeReaderResult.FromSuccess(user);
			}
			else if (!CreateIfNotFound) //Profile doesn't exist and this is something like checking their harem
			{
				return TypeReaderResult.FromError(CommandError.ObjectNotFound,
					$"{user.Format()} does not have a profile.");
			}
			else if (user is not IGuildUser guildUser)
			{
				return TypeReaderResult.FromError(CommandError.ObjectNotFound,
					$"{user.Format()} is not in the guild.");
			}
			else
			{
				var newEntry = new User(guildUser);
				await db.AddUserAsync(newEntry).CAF();
				return TypeReaderResult.FromSuccess(newEntry);
			}
		}
	}
}