using Advobot.Actions.Formatting;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Advobot.Classes
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
			Title = title.FormatTitle();
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
		public static string FormatMultiple(bool haveEqualSpacing, params LogCounter[] input)
		{
			if (haveEqualSpacing)
			{
				var leftSpacing = input.Max(x => x.Count).GetLengthOfNumber();
				var rightSpacing = input.Select(x => x.Title.Length).Max() + 1;

				var sb = new StringBuilder();
				foreach (var count in input)
				{
					sb.AppendLineFeed(GeneralFormatting.FormatStringsWithLength(count.Title, count.Count, rightSpacing, leftSpacing));
				}
				return sb.ToString();
			}
			else
			{
				var sb = new StringBuilder();
				foreach (var count in input)
				{
					sb.AppendLineFeed(count.ToString());
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