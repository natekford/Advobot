using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.Formatting
{
	/// <summary>
	/// Converts certain arguments into discord specific arguments then formats the string.
	/// </summary>
	public class DiscordFormattableString : IDiscordFormattableString
	{
		private readonly object?[] _Args;
		private readonly string _Format;

		/// <summary>
		/// Creates an instance of <see cref="DiscordFormattableString"/> mimicking <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		public DiscordFormattableString(FormattableString source)
		{
			_Format = source.Format;
			_Args = source.GetArguments();
		}

		/// <summary>
		/// Creates an instance of <see cref="DiscordFormattableString"/>.
		/// </summary>
		/// <param name="value"></param>
		public DiscordFormattableString(object? value)
		{
			_Format = "{0}";
			_Args = new[] { value };
		}

		/// <inheritdoc />
		public override string ToString()
			=> ToString(null);

		/// <inheritdoc />
		public string ToString(IFormatProvider? formatProvider)
			=> string.Format(formatProvider, _Format, _Args);

		/// <inheritdoc />
		public string ToString(BaseSocketClient client, SocketGuild guild, IFormatProvider? formatProvider)
		{
			var converted = _Args.Select(x => ConvertArgument(client, guild, x));
			return string.Format(formatProvider, _Format, converted);
		}

		/// <inheritdoc />
		public async Task<string> ToStringAsync(IDiscordClient client, IGuild guild, IFormatProvider? formatProvider)
		{
			if (client is BaseSocketClient socketClient && guild is SocketGuild socketGuild)
			{
				return ToString(socketClient, socketGuild, formatProvider);
			}

			var converted = new object?[_Args.Length];
			for (var i = 0; i < _Args.Length; ++i)
			{
				converted[i] = await ConvertArgumentAsync(client, guild, _Args[i]).CAF();
			}
			return string.Format(formatProvider, _Format, converted);
		}

		string IFormattable.ToString(string format, IFormatProvider formatProvider)
			=> ToString(formatProvider);

		private static object? ConvertArgument(BaseSocketClient c, SocketGuild g, object? value) => value switch
		{
			ulong id => GetSnowflakeEntity(c, g, id),
			IEnumerable enumerable => ConvertEnumerable(c, g, enumerable),
			_ => value,
		};

		private static async Task<object?> ConvertArgumentAsync(IDiscordClient c, IGuild g, object? value) => value switch
		{
			ulong id => await GetSnowflakeEntityAsync(c, g, id).CAF(),
			IEnumerable enumerable => await ConvertEnumerableAsync(c, g, enumerable).CAF(),
			_ => value,
		};

		private static IEnumerable ConvertEnumerable(BaseSocketClient c, SocketGuild g, IEnumerable source)
		{
			foreach (var obj in source)
			{
				yield return ConvertArgument(c, g, obj);
			}
		}

		private static async Task<IEnumerable> ConvertEnumerableAsync(IDiscordClient c, IGuild g, IEnumerable source)
		{
			var output = new List<object?>();
			foreach (var obj in source)
			{
				output.Add(await ConvertArgumentAsync(c, g, obj).CAF());
			}
			return output;
		}

		private static ISnowflakeEntity? GetSnowflakeEntity(BaseSocketClient c, SocketGuild g, ulong id) => id switch
		{
			_ when g?.Id == id => g,
			_ when g?.GetRole(id) is ISnowflakeEntity role => role,
			_ when g?.GetChannel(id) is ISnowflakeEntity channel => channel,
			_ when g?.GetUser(id) is ISnowflakeEntity user => user,
			_ when c?.GetUser(id) is ISnowflakeEntity user => user,
			_ when c?.GetGuild(id) is ISnowflakeEntity guild => guild,
			_ => null,
		};

		private static async Task<ISnowflakeEntity?> GetSnowflakeEntityAsync(IDiscordClient c, IGuild g, ulong id) => id switch
		{
			_ when g != null && g.Id == id => g,
			_ when g?.GetRole(id) is ISnowflakeEntity role => role,
			_ when g != null && await g.GetChannelAsync(id).CAF() is ISnowflakeEntity channel => channel,
			_ when g != null && await g.GetUserAsync(id).CAF() is ISnowflakeEntity user => user,
			_ when c != null && await c.GetUserAsync(id).CAF() is ISnowflakeEntity user => user,
			_ when c != null && await c.GetGuildAsync(id).CAF() is ISnowflakeEntity guild => guild,
			_ => null,
		};
	}
}