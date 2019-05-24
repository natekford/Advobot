using Discord;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Classes.Formatting
{
	/// <summary>
	/// Converts certain arguments into discord specific arguments then formats the string.
	/// </summary>
	public class DiscordFormattableStringCollection : ICollection<FormattableString>, IDiscordFormattableString
	{
		/// <inheritdoc />
		public int Count => _Source.Count;
		/// <inheritdoc />
		public bool IsReadOnly => _Source.IsReadOnly;

		private readonly ICollection<FormattableString> _Source;

		/// <summary>
		/// Creates an instance of <see cref="FormattableString"/>.
		/// </summary>
		/// <param name="source"></param>
		public DiscordFormattableStringCollection(IEnumerable<FormattableString> source)
		{
			_Source = source.ToList();
		}
		/// <summary>
		/// Creates an instance of <see cref="FormattableString"/>.
		/// </summary>
		/// <param name="source"></param>
		public DiscordFormattableStringCollection(params FormattableString[] source) : this((IEnumerable<FormattableString>)source) { }

		/// <inheritdoc />
		public void Add(FormattableString item)
			=> _Source.Add(item);
		/// <inheritdoc />
		public void Clear()
			=> _Source.Clear();
		/// <inheritdoc />
		public bool Contains(FormattableString item)
			=> _Source.Contains(item);
		/// <inheritdoc />
		public void CopyTo(FormattableString[] array, int arrayIndex)
			=> _Source.CopyTo(array, arrayIndex);
		/// <inheritdoc />
		public IEnumerator<FormattableString> GetEnumerator()
			=> _Source.GetEnumerator();
		/// <inheritdoc />
		public bool Remove(FormattableString item)
			=> _Source.Remove(item);

		/// <inheritdoc />
		public string ToString(IFormatProvider formatProvider)
		{
			var sb = new StringBuilder();
			foreach (var item in _Source)
			{
				sb.Append(item.ToString(formatProvider));
			}
			return sb.ToString();
		}
		/// <inheritdoc />
		public string ToString(IFormatProvider formatProvider, BaseSocketClient client, SocketGuild guild)
		{
			var sb = new StringBuilder();
			foreach (var item in _Source)
			{
				sb.Append(new DiscordFormattableString(item).ToString(formatProvider, client, guild));
			}
			return sb.ToString();
		}
		/// <inheritdoc />
		public async Task<string> ToStringAsync(IFormatProvider formatProvider, IDiscordClient client, IGuild guild)
		{
			if (client is BaseSocketClient socketClient && guild is SocketGuild socketGuild)
			{
				return ToString(formatProvider, socketClient, socketGuild);
			}

			var sb = new StringBuilder();
			foreach (var item in _Source)
			{
				sb.Append(await new DiscordFormattableString(item).ToStringAsync(formatProvider, client, guild));
			}
			return sb.ToString();
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() => _Source.GetEnumerator();
	}
}
