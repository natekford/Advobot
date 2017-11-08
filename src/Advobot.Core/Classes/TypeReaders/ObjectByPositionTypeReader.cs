using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.TypeReaders
{
	/// <summary>
	/// Gets the object of type <typeparamref name="T"/> with the given position.
	/// Only works for <see cref="IRole"/>, <see cref="ITextChannel"/>, <see cref="IVoiceChannel"/>, and <see cref="IGuildChannel"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class ObjectByPositionTypeReader<T> : TypeReader
	{
		private static Dictionary<Type, Func<IGuild, int, IEnumerable<object>>> _ObjectGatherers = new Dictionary<Type, Func<IGuild, int, IEnumerable<object>>>
		{
			{ typeof(IRole), (guild, position) => guild.Roles.Where(x => x.Position == position) },
			{ typeof(ITextChannel), (guild, position) => (guild as SocketGuild).TextChannels.Where(x => x.Position == position) },
			{ typeof(IVoiceChannel), (guild, position) => (guild as SocketGuild).VoiceChannels.Where(x => x.Position == position) },
			{ typeof(IGuildChannel), (guild, position) => (guild as SocketGuild).Channels.Where(x => x.Position == position) },
		};

		/// <summary>
		/// Converts the input to a number then tries to find the role with that position.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (!uint.TryParse(input, out var position))
			{
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Unable to parse the input as a uint."));
			}

			var objects = _ObjectGatherers.TryGetValue(typeof(T), out var f) ? f(context.Guild, (int)position) : Enumerable.Empty<object>();
			if (!objects.Any())
			{
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"No object has the position `{position}`."));
			}
			else if (objects.Count() > 1)
			{
				return Task.FromResult(TypeReaderResult.FromError(CommandError.MultipleMatches, $"Multiple objects have the position `{position}`."));
			}
			else
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(objects.First()));
			}
		}
	}
}
