using Advobot.Core.Utilities;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Used in logging. Holds the name of what is being logged and the count.
	/// </summary>
	public sealed class LogCounter : INotifyPropertyChanged
	{
		public string Title { get; private set; }
		private int _Count;
		public int Count => _Count;

		public event PropertyChangedEventHandler PropertyChanged;

		public LogCounter([CallerMemberName] string title = "")
		{
			Title = title.FormatTitle().Trim();
		}

		public void Add(int count)
		{
			Interlocked.Add(ref _Count, count);
			NotifyPropertyChanged(nameof(Count));
		}
		public void Remove(int count)
		{
			Interlocked.Add(ref _Count, -count);
			NotifyPropertyChanged(nameof(Count));
		}
		public void Increment()
		{
			Interlocked.Increment(ref _Count);
			NotifyPropertyChanged(nameof(Count));
		}
		public void Decrement()
		{
			Interlocked.Decrement(ref _Count);
			NotifyPropertyChanged(nameof(Count));
		}
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
				? counters.Select(x => (Title: $"**{x.Title}**:", Count: $"`{x.Count}`"))
				: counters.Select(x => (Title: $"{x.Title}:", Count: $"{x.Count}"))).ToList();

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

		public override string ToString()
		{
			return $"**{Title}:** {Count}";
		}
	}
}