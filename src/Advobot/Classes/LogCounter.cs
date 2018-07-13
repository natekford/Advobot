using AdvorangesUtils;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Advobot.Classes
{
	/// <summary>
	/// Used in logging. Holds the name of what is being logged and the count.
	/// </summary>
	public sealed class LogCounter : INotifyPropertyChanged
	{
		/// <summary>
		/// The title of the log counter.
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// How many instances have been logged.
		/// </summary>
		public int Count => _Count;

		private int _Count;

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Creates an instance of <see cref="LogCounter"/>.
		/// </summary>
		/// <param name="name"></param>
		public LogCounter([CallerMemberName] string name = "")
		{
			Name = name.FormatTitle().Trim();
		}

		/// <summary>
		/// Add a specified amount to the counter.
		/// </summary>
		/// <param name="count"></param>
		public void Add(int count)
		{
			Interlocked.Add(ref _Count, count);
			NotifyPropertyChanged(nameof(Count));
		}
		/// <summary>
		/// Remove a specified amount from the counter.
		/// </summary>
		/// <param name="count"></param>
		public void Remove(int count)
		{
			Interlocked.Add(ref _Count, -count);
			NotifyPropertyChanged(nameof(Count));
		}
		/// <summary>
		/// Fires the property changed event.
		/// </summary>
		/// <param name="propertyName"></param>
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		/// <summary>
		/// Return a formatted string in which the format is each counter on a new line, or if 
		/// <paramref name="haveEqualSpacing"/> is true there will always be an equal amount of space between each
		/// title and count.
		/// </summary>
		/// <param name="withMarkDown"></param>
		/// <param name="haveEqualSpacing"></param>
		/// <param name="counters"></param>
		/// <returns></returns>
		public static string FormatMultiple(bool withMarkDown, bool haveEqualSpacing, params LogCounter[] counters)
		{
			var titlesAndCount = (withMarkDown
				? counters.Select(x => (Title: $"**{x.Name}**:", Count: $"`{x.Count}`"))
				: counters.Select(x => (Title: $"{x.Name}:", Count: $"{x.Count}"))).ToList();

			if (haveEqualSpacing)
			{
				var rightSpacing = titlesAndCount.Select(x => x.Title.Length).DefaultIfEmpty(0).Max() + 1;
				var leftSpacing = titlesAndCount.Select(x => x.Count.Length).DefaultIfEmpty(0).Max();

				var sb = new StringBuilder();
				foreach (var tc in titlesAndCount)
				{
					var str1 = tc.Title.ToString().PadRight(Math.Max(rightSpacing, 0));
					var str2 = tc.Count.ToString().PadLeft(Math.Max(leftSpacing, 0));
					sb.AppendLineFeed($"{str1}{str2}");
				}
				return sb.ToString();
			}
			else
			{
				var sb = new StringBuilder();
				foreach (var tc in titlesAndCount)
				{
					sb.AppendLineFeed($"{tc.Title} {tc.Count}");
				}
				return sb.ToString();
			}
		}
		/// <summary>
		/// Returns the title and count.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"**{Name}:** {Count}";
		}
	}
}