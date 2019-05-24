using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Classes.Formatting
{
	/// <summary>
	/// Converts certain arguments into discord specific arguments then formats the string.
	/// </summary>
	public class DiscordFormattableString : IDiscordFormattableString
	{
		private readonly FormattableString _Source;

		/// <summary>
		/// Creates an instance of <see cref="DiscordFormattableString"/>.
		/// </summary>
		/// <param name="source"></param>
		public DiscordFormattableString(FormattableString source)
		{
			_Source = source;
		}

		/// <inheritdoc />
		public string ToString(IFormatProvider formatProvider)
			=> _Source.ToString(formatProvider);

		/// <inheritdoc />
		public string ToString(IFormatProvider formatProvider, BaseSocketClient client, SocketGuild guild)
		{
			var converted = _Source.GetArguments().Select(x => ConvertArgument(client, guild, x));
			return string.Format(formatProvider, _Source.Format, converted);
		}
		private static object? ConvertArgument(BaseSocketClient c, SocketGuild g, object? value) => value switch
		{
			//TODO: IDictionary support?
			ulong id => GetSnowflakeEntity(c, g, id),
			IEnumerable enumerable => ConvertEnumerable(c, g, enumerable),
			_ => value,
		};
		private static ISnowflakeEntity? GetSnowflakeEntity(BaseSocketClient c, SocketGuild g, ulong id) => id switch
		{
			_ when g != null && g.Id == id => g,
			_ when g != null && g.GetRole(id) is ISnowflakeEntity role => role,
			_ when g != null && g.GetChannel(id) is ISnowflakeEntity channel => channel,
			_ when g != null && g.GetUser(id) is ISnowflakeEntity user => user,
			_ when c != null && c.GetUser(id) is ISnowflakeEntity user => user,
			_ when c != null && c.GetGuild(id) is ISnowflakeEntity guild => guild,
			_ => null,
		};
		private static IEnumerable ConvertEnumerable(BaseSocketClient c, SocketGuild g, IEnumerable source)
		{
			foreach (var obj in source)
			{
				yield return ConvertArgument(c, g, obj);
			}
		}

		/// <inheritdoc />
		public async Task<string> ToStringAsync(IFormatProvider formatProvider, IDiscordClient client, IGuild guild)
		{
			if (client is BaseSocketClient socketClient && guild is SocketGuild socketGuild)
			{
				return ToString(formatProvider, socketClient, socketGuild);
			}

			var tasks = _Source.GetArguments().Select(x => ConvertArgumentAsync(client, guild, x));
			var converted = await Task.WhenAll(tasks).CAF();
			return string.Format(formatProvider, _Source.Format, converted);
		}
		private static async Task<object?> ConvertArgumentAsync(IDiscordClient c, IGuild g, object? value) => value switch
		{
			ulong id => await GetSnowflakeEntityAsync(c, g, id).CAF(),
			IEnumerable enumerable => await ConvertEnumerableAsync(c, g, enumerable).CAF(),
			_ => value,
		};
		private static async Task<ISnowflakeEntity?> GetSnowflakeEntityAsync(IDiscordClient c, IGuild g, ulong id) => id switch
		{
			_ when g != null && g.Id == id => g,
			_ when g != null && g.GetRole(id) is ISnowflakeEntity role => role,
			_ when g != null && await g.GetChannelAsync(id).CAF() is ISnowflakeEntity channel => channel,
			_ when g != null && await g.GetUserAsync(id).CAF() is ISnowflakeEntity user => user,
			_ when c != null && await c.GetUserAsync(id).CAF() is ISnowflakeEntity user => user,
			_ when c != null && await c.GetGuildAsync(id).CAF() is ISnowflakeEntity guild => guild,
			_ => null,
		};
		private static async Task<IEnumerable> ConvertEnumerableAsync(IDiscordClient c, IGuild g, IEnumerable source)
		{
			var output = new List<object>();
			foreach (var obj in source)
			{
				output.Add(await ConvertArgumentAsync(c, g, obj).CAF());
			}
			return output;
		}
	}
}
