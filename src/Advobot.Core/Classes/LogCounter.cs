using Advobot.Core.Actions.Formatting;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Used in logging. Holds the name of what is being logged and the count.
	/// </summary>
	public class LogCounter
	{
		public string Title { get; private set; }
		public int Count { get; private set; }

		public LogCounter([CallerMemberName] string title = "")
		{
			Title = title.FormatTitle().Trim();
			Count = 0;
		}

		public void Add(int count)
		{
			Count += count;
		}
		public void Remove(int count)
		{
			Count -= count;
		}
		public void Increment()
		{
			Add(1);
		}
		public void Decrement()
		{
			Remove(1);
		}

		/// <summary>
		/// Return a formatted string in which the format is each <see cref="ActionCount.ToString()"/> on a new line, or if 
		/// <paramref name="haveEqualSpacing"/> is true there will always be an equal amount of space between each
		/// title and count.
		/// </summary>
		/// <param name="haveEqualSpacing"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string FormatMultiple(bool haveEqualSpacing, params LogCounter[] counters)
		{
			var titlesAndCount = counters.Select(x => (Title: $"**{x.Title}**:", Count: $"`{x.Count}`"));

			if (haveEqualSpacing)
			{
				var rightSpacing = titlesAndCount.Select(x => x.Title.Length).DefaultIfEmpty(0).Max();
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