using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Advobot.Core.Classes.CloseWords
{
	/// <summary>
	/// Container of close words which is intended to be removed after the time returns a value less than the current time.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class CloseWords<T> : ITime
	{
		public DateTime Time { get; private set; }
		public ulong GuildId { get; private set; }
		public ulong ChannelId { get; private set; }
		public ulong UserId { get; private set; }
		public ulong MessageId { get; private set; }
		public ImmutableList<CloseWord> List { get; private set; }

		private int _MaxAllowedCloseness = 4;
		private int _MaxOutput = 5;

		protected CloseWords(TimeSpan time, ICommandContext context, IEnumerable<T> objects, string input)
		{
			GuildId = context.Guild.Id;
			ChannelId = context.Channel.Id;
			UserId = context.User.Id;
			MessageId = context.Message.Id;
			List = GetObjectsWithSimilarNames(objects.ToList(), input).ToImmutableList();
			Time = DateTime.UtcNow.Add(time.Equals(default) ? Constants.DEFAULT_WAIT_TIME : time);
		}

		protected abstract CloseWord FindCloseWord(T obj, string input);
		protected abstract CloseWord FindCloseWord(IEnumerable<T> objs, IEnumerable<string> alreadyUsedNames, string input);
		protected int FindCloseName(string source, string target, int threshold = 10)
		{
			/* Damerau Levenshtein Distance: https://en.wikipedia.org/wiki/Damerau–Levenshtein_distance
			 * Copied nearly verbatim from: https://stackoverflow.com/a/9454016 
			 */
			var length1 = source.Length;
			var length2 = target.Length;

			// Return trivial case - difference in string lengths exceeds threshhold
			if (Math.Abs(length1 - length2) > threshold) { return int.MaxValue; }

			// Ensure arrays [i] / length1 use shorter length 
			if (length1 > length2)
			{
				Swap(ref target, ref source);
				Swap(ref length1, ref length2);
			}

			var maxi = length1;
			var maxj = length2;

			var dCurrent = new int[maxi + 1];
			var dMinus1 = new int[maxi + 1];
			var dMinus2 = new int[maxi + 1];
			int[] dSwap;

			for (var i = 0; i <= maxi; i++) { dCurrent[i] = i; }

			int jm1 = 0, im1 = 0, im2 = -1;

			for (var j = 1; j <= maxj; j++)
			{

				// Rotate
				dSwap = dMinus2;
				dMinus2 = dMinus1;
				dMinus1 = dCurrent;
				dCurrent = dSwap;

				// Initialize
				var minDistance = int.MaxValue;
				dCurrent[0] = j;
				im1 = 0;
				im2 = -1;

				for (var i = 1; i <= maxi; i++)
				{

					var cost = source[im1] == target[jm1] ? 0 : 1;

					var del = dCurrent[im1] + 1;
					var ins = dMinus1[i] + 1;
					var sub = dMinus1[im1] + cost;

					//Fastest execution for min value of 3 integers
					var min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

					if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
					{
						min = Math.Min(min, dMinus2[im2] + cost);
					}

					dCurrent[i] = min;
					if (min < minDistance) { minDistance = min; }
					im1++;
					im2++;
				}
				jm1++;
				if (minDistance > threshold) { return int.MaxValue; }
			}

			var result = dCurrent[maxi];
			return (result > threshold) ? int.MaxValue : result;
		}
		private List<CloseWord> GetObjectsWithSimilarNames(List<T> suppliedObjects, string input)
		{
			var closeWords = new List<CloseWord>();
			//First loop around to find words that are similar
			foreach (var word in suppliedObjects)
			{
				var closeWord = FindCloseWord(word, input);
				if (closeWord.Closeness > _MaxAllowedCloseness)
				{
					continue;
				}

				closeWords.Add(closeWord);
				if (closeWords.Count > _MaxOutput)
				{
					closeWords = closeWords.OrderBy(x => x.Closeness).ToList();
					closeWords.RemoveRange(_MaxOutput, closeWords.Count - _MaxOutput);
				}
			}

			if (closeWords.Count < _MaxOutput)
			{
				for (int i = closeWords.Count - 1; i < _MaxOutput; ++i)
				{
					closeWords.Add(FindCloseWord(suppliedObjects, closeWords.Select(x => x.Name), input));
				}
			}
			return closeWords;
		}
		private void Swap<T2>(ref T2 arg1, ref T2 arg2)
		{
			var temp = arg1;
			arg1 = arg2;
			arg2 = temp;
		}

		/// <summary>
		/// Holds an object which has a name and text and its closeness.
		/// </summary>
		public struct CloseWord
		{
			public int Closeness { get; private set; }
			public string Name { get; private set; }
			public string Text { get; private set; }

			public CloseWord(int closeness, string name, string text)
			{
				Closeness = closeness;
				Name = name;
				Text = text;
			}
		}
	}
}