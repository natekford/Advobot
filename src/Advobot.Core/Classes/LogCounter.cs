using Advobot.Core.Utilities.Formatting;
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
	public class LogCounter : INotifyPropertyChanged
	{
		public string Title { get; private set; }
		private int _Count = 0;
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
		/// Return a formatted string in which the format is each <see cref="ActionCount.ToString()"/> on a new line, or if 
		/// <paramref name="haveEqualSpacing"/> is true there will always be an equal amount of space between each
		/// title and count.
		/// </summary>
		/// <param name="haveEqualSpacing"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string FormatMultiple(bool withMarkDown, bool haveEqualSpacing, params LogCounter[] counters)
		{
			var titlesAndCount = withMarkDown
				? counters.Select(x => (Title: $"**{x.Title}**:", Count: $"`{x.Count}`"))
				: counters.Select(x => (Title: $"{x.Title}:", Count: $"{x.Count}"));

			if (haveEqualSpacing)
			{
				var rightSpacing = titlesAndCount.Select(x => x.Title.Length).DefaultIfEmpty(0).Max() + 1;
				var leftSpacing = titlesAndCount.Select(x => x.Count.Length).DefaultIfEmpty(0).Max();

				var sb = new StringBuilder();
				foreach (var tc in titlesAndCount)
				{
					var str = GeneralFormatting.FormatStringsWithLength(tc.Title, tc.Count, rightSpacing, leftSpacing);
					sb.AppendLineFeed(str);
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