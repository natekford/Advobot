using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Rules
{
	public class RuleHolder : IGuildSetting
	{
		[JsonProperty]
		public Dictionary<string, List<string>> Categories = new Dictionary<string, List<string>>();

		/// <summary>
		/// Sends the rules to the specified channel.
		/// </summary>
		/// <param name="categories"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public async Task<IEnumerable<IUserMessage>> SendAsync(RuleFormatter formatter, IMessageChannel channel)
		{
			var messages = new List<IUserMessage>();

			var formattedCategories = Categories.Select(x => ToString(formatter, x.Key)).ToList();
			var formattedRules = String.Join("\n", formattedCategories);
			//If all of the rules can be sent in one message, do that.
			if (!String.IsNullOrWhiteSpace(formattedRules) && formattedRules.Length <= 2000)
			{
				messages.Add(await MessageUtils.SendMessageAsync(channel, formattedRules).CAF());
				return messages;
			}

			//If not, go by category
			foreach (var category in formattedCategories)
			{
				messages.AddRange(await PrivateSendCategoryAsync(category, channel).CAF());
			}
			return messages;
		}
		/// <summary>
		/// Sends a category to the specified channel.
		/// </summary>
		/// <param name="formattedCategory"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public async Task<IEnumerable<IUserMessage>> SendCategoryAsync(RuleFormatter formatter, string category, IMessageChannel channel)
		{
			return await PrivateSendCategoryAsync(ToString(formatter, category), channel).CAF();
		}
		private async Task<IEnumerable<IUserMessage>> PrivateSendCategoryAsync(string formattedCategory, IMessageChannel channel)
		{
			var messages = new List<IUserMessage>();
			//Null category gets ignored
			if (String.IsNullOrWhiteSpace(formattedCategory))
			{
				return messages;
			}
			//Short enough categories just get sent on their own
			if (formattedCategory.Length <= 2000)
			{
				messages.Add(await MessageUtils.SendMessageAsync(channel, formattedCategory).CAF());
				return messages;
			}

			var sb = new StringBuilder();
			foreach (var part in formattedCategory.Split('\n'))
			{
				//If the current stored text + the new part is too big, send the current stored text
				//Then start building new stored text to send
				if (sb.Length + part.Length >= 2000)
				{
					messages.Add(await MessageUtils.SendMessageAsync(channel, sb.ToString()).CAF());
					sb.Clear();
				}
				sb.Append(part);
			}
			//Send the last remaining text
			if (sb.Length > 0)
			{
				messages.Add(await MessageUtils.SendMessageAsync(channel, sb.ToString()).CAF());
			}
			return messages;
		}

		public override string ToString()
		{
			return ToString(new RuleFormatter());
		}
		public string ToString(RuleFormatter formatter)
		{
			var sb = new StringBuilder();
			var index = 0;
			foreach (var kvp in Categories)
			{
				sb.AppendLineFeed(formatter.FormatName(kvp.Key));
				for (var r = 0; r < kvp.Value.Count; ++r)
				{
					sb.AppendLineFeed(formatter.FormatRule(kvp.Value[r], r, kvp.Value.Count));
				}
				++index;
			}
			return sb.ToString();
		}
		public string ToString(RuleFormatter formatter, string category)
		{
			var c = Categories[category];
			var sb = new StringBuilder();
			sb.AppendLineFeed(formatter.FormatName(category));
			for (var r = 0; r < c.Count; ++r)
			{
				sb.AppendLineFeed(formatter.FormatRule(c[r], r, c.Count));
			}
			return sb.ToString();
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}
