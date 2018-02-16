using Advobot.Core.Interfaces;
using Discord.Commands;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Classes.CloseWords
{
	/// <summary>
	/// Container of close words which is intended to be removed after the time has passed.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class CloseWords<T> : ITime
	{
		/// <summary>
		/// The max allowed closeness before a word will not be added.
		/// </summary>
		public static int MaxAllowedCloseness = 4;
		/// <summary>
		/// The max allowed output to add to <see cref="List"/>.
		/// </summary>
		public static int MaxOutput = 5;

		/// <summary>
		/// The id of the object for LiteDB.
		/// </summary>
		public ObjectId Id { get; set; }
		/// <summary>
		/// The time to remove this object and delete whatever message can be gotten from <see cref="MessageId"/>.
		/// </summary>
		public DateTime Time { get; set; }
		/// <summary>
		/// The id of the guild from the passed in context.
		/// </summary>
		public ulong GuildId { get; set; }
		/// <summary>
		/// The id of the channel from the passed in context.
		/// </summary>
		public ulong ChannelId { get; set; }
		/// <summary>
		/// The id of the user from the passed in context.
		/// </summary>
		public ulong UserId { get; set; }
		/// <summary>
		/// The id of the response the bot has sent.
		/// </summary>
		public ulong MessageId { get; set; }
		/// <summary>
		/// The gathered words.
		/// </summary>
		public List<CloseWord> List { get; set; }

		protected CloseWords() { }
		protected CloseWords(TimeSpan time, ICommandContext context, IEnumerable<T> objects, string input)
		{
			GuildId = context.Guild.Id;
			ChannelId = context.Channel.Id;
			UserId = context.User.Id;
			List = GetObjectsWithSimilarNames(objects.ToList(), input);
			Time = DateTime.UtcNow.Add(time.Equals(default) ? Constants.DEFAULT_WAIT_TIME : time);
		}

		/// <summary>
		/// Finds an object with a similar name to the search term..
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		protected abstract CloseWord FindCloseWord(T obj, string search);
		/// <summary>
		/// Finds an object with the search term in directly in their name.
		/// </summary>
		/// <param name="objs"></param>
		/// <param name="alreadyUsedNames"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		protected abstract CloseWord FindCloseWord(IEnumerable<T> objs, IEnumerable<string> alreadyUsedNames, string search);
		/// <summary>
		/// Returns a value gotten from using Damerau Levenshtein distance to compare the source and target.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="threshold"></param>
		/// <returns></returns>
		protected int FindCloseName(string source, string target, int threshold = 10)
		{
			void Swap<T2>(ref T2 arg1, ref T2 arg2)
			{
				var temp = arg1;
				arg1 = arg2;
				arg2 = temp;
			}

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
				if (closeWord.Closeness > MaxAllowedCloseness)
				{
					continue;
				}

				closeWords.Add(closeWord);
				if (closeWords.Count > MaxOutput)
				{
					closeWords = closeWords.OrderBy(x => x.Closeness).ToList();
					closeWords.RemoveRange(MaxOutput, closeWords.Count - MaxOutput);
				}
			}

			if (closeWords.Count < MaxOutput)
			{
				for (int i = closeWords.Count - 1; i < MaxOutput; ++i)
				{
					closeWords.Add(FindCloseWord(suppliedObjects, closeWords.Select(x => x.Name), input));
				}
			}
			return closeWords.Where(x => x.Closeness > -1).ToList();
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