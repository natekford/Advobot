using AdvorangesUtils;
using Discord;
using System;
using System.Threading.Tasks;

namespace Advobot.Gacha.Utils
{
	public static class GachaUtils
	{
		public static async Task<bool> SafeAddReactionsAsync(this IUserMessage message, params Emoji[] emojis)
		{
			try
			{
				await message.AddReactionsAsync(emojis).CAF();
				return true;
			}
			catch (Exception e)
			{
				throw new NotImplementedException();
				return false;
			}
		}
	}
}
