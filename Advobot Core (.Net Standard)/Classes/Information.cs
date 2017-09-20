﻿using Advobot.Enums;
using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes
{
	/// <summary>
	/// Holds a name and description.
	/// </summary>
	public class Quote : ISetting, IDescription
	{
		[JsonProperty]
		public string Name { get; }
		[JsonProperty]
		public string Description { get; }

		public Quote(string name, string description)
		{
			Name = name;
			Description = description;
		}

		public override string ToString()
		{
			return $"`{Name}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}

	/// <summary>
	/// Holds information about a command, such as its name, aliases, usage, base permissions, description, category, and default enabled value.
	/// </summary>
	public class HelpEntry : IDescription
	{
		private const string PLACE_HOLDER_STR = "N/A";
		public string Name { get; }
		public string[] Aliases { get; }
		public string Usage { get; }
		public string BasePerm { get; }
		public string Description { get; }
		public CommandCategory Category { get; }
		public bool DefaultEnabled { get; }

		public HelpEntry(string name, string[] aliases, string usage, string basePerm, string description, CommandCategory category, bool defaultEnabled)
		{
			Name = String.IsNullOrWhiteSpace(name) ? PLACE_HOLDER_STR : name;
			Aliases = aliases ?? new[] { PLACE_HOLDER_STR };
			Usage = String.IsNullOrWhiteSpace(usage) ? PLACE_HOLDER_STR : Constants.PLACEHOLDER_PREFIX + usage;
			BasePerm = String.IsNullOrWhiteSpace(basePerm) ? PLACE_HOLDER_STR : basePerm;
			Description = String.IsNullOrWhiteSpace(description) ? PLACE_HOLDER_STR : description;
			Category = category;
			DefaultEnabled = defaultEnabled;
		}

		public override string ToString()
		{
			var aliasStr = $"**Aliases:** {String.Join(", ", Aliases)}";
			var usageStr = $"**Usage:** {Usage}";
			var permStr = $"\n**Base Permission(s):**\n{BasePerm}";
			var descStr = $"\n**Description:**\n{Description}";
			return String.Join("\n", new[] { aliasStr, usageStr, permStr, descStr });
		}
	}

	/// <summary>
	/// Container of close words which is intended to be removed after <see cref="GetTime()"/> returns a value less than the current time.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CloseWords<T> : ITimeInterface where T : IDescription
	{
		public ulong UserId { get; }
		public IReadOnlyCollection<CloseWord> List { get; }
		private DateTime _Time;

		public CloseWords(ulong userID, IEnumerable<T> suppliedObjects, string input)
		{
			UserId = userID;
			List = GetObjectsWithSimilarNames(suppliedObjects, input);
			_Time = DateTime.UtcNow.AddSeconds(Constants.SECONDS_ACTIVE_CLOSE);
		}

		private static IReadOnlyCollection<CloseWord> GetObjectsWithSimilarNames(IEnumerable<T> suppliedObjects, string input)
		{
			var closeWords = new List<CloseWord>();
			//First loop around to find words that are similar
			foreach (var word in suppliedObjects)
			{
				var closeness = FindCloseName(word.Name, input);
				if (closeness > 3)
				{
					continue;
				}

				closeWords.Add(new CloseWord(word, closeness));
				if (closeWords.Count > 5)
				{
					closeWords.OrderBy(x => x.Closeness);
					closeWords.RemoveRange(4, closeWords.Count - 4);
				}
			}

			//Second loop around to find words that have the search term in them
			foreach (var word in suppliedObjects.Where(x => x.Name.CaseInsContains(input)))
			{
				if (closeWords.Count >= 5)
				{
					break;
				}
				else if (!closeWords.Any(x => x.Word.Name.CaseInsEquals(word.Name)))
				{
					closeWords.Add(new CloseWord(word, 5));
				}
			}

			return closeWords.AsReadOnly();
		}
		private static int FindCloseName(string source, string target, int threshold = 10)
		{
			/* Damerau Levenshtein Distance: https://en.wikipedia.org/wiki/Damerau–Levenshtein_distance
			 * Copied verbatim from: https://stackoverflow.com/a/9454016 
			 */
			int length1 = source.Length;
			int length2 = target.Length;

			// Return trivial case - difference in string lengths exceeds threshhold
			if (Math.Abs(length1 - length2) > threshold) { return int.MaxValue; }

			// Ensure arrays [i] / length1 use shorter length 
			if (length1 > length2)
			{
				Swap(ref target, ref source);
				Swap(ref length1, ref length2);
			}

			int maxi = length1;
			int maxj = length2;

			int[] dCurrent = new int[maxi + 1];
			int[] dMinus1 = new int[maxi + 1];
			int[] dMinus2 = new int[maxi + 1];
			int[] dSwap;

			for (int i = 0; i <= maxi; i++) { dCurrent[i] = i; }

			int jm1 = 0, im1 = 0, im2 = -1;

			for (int j = 1; j <= maxj; j++)
			{

				// Rotate
				dSwap = dMinus2;
				dMinus2 = dMinus1;
				dMinus1 = dCurrent;
				dCurrent = dSwap;

				// Initialize
				int minDistance = int.MaxValue;
				dCurrent[0] = j;
				im1 = 0;
				im2 = -1;

				for (int i = 1; i <= maxi; i++)
				{

					int cost = source[im1] == target[jm1] ? 0 : 1;

					int del = dCurrent[im1] + 1;
					int ins = dMinus1[i] + 1;
					int sub = dMinus1[im1] + cost;

					//Fastest execution for min value of 3 integers
					int min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

					if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
						min = Math.Min(min, dMinus2[im2] + cost);

					dCurrent[i] = min;
					if (min < minDistance) { minDistance = min; }
					im1++;
					im2++;
				}
				jm1++;
				if (minDistance > threshold) { return int.MaxValue; }
			}

			int result = dCurrent[maxi];
			return (result > threshold) ? int.MaxValue : result;
		}
		private static void Swap<T2>(ref T2 arg1, ref T2 arg2)
		{
			T2 temp = arg1;
			arg1 = arg2;
			arg2 = temp;
		}

		public DateTime GetTime() => _Time;

		/// <summary>
		/// Holds an object which has a name and text and its closeness.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public struct CloseWord
		{
			public T Word { get; }
			public int Closeness { get; }

			public CloseWord(T word, int closeness)
			{
				Word = word;
				Closeness = closeness;
			}
		}
	}
}
