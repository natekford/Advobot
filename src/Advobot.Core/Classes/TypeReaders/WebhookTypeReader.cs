using AdvorangesUtils;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to find an <see cref="IWebhook"/> on a guild.
	/// </summary>
	public sealed class WebhookTypeReader : TypeReader
    {
		/// <summary>
		/// Checks for any webhooks matching the input. Input is tested as a webhook id and a username.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			IWebhook wh = null;
			var webhooks = await context.Guild.GetWebhooksAsync().CAF();
			if (ulong.TryParse(input, out var id))
			{
				wh = webhooks.FirstOrDefault(x => x.Id == id);
			}
			if (wh == null)
			{
				var matchingWebhooks = webhooks.Where(x => x.Name.CaseInsEquals(input)).ToList();
				if (matchingWebhooks.Count == 1)
				{
					wh = matchingWebhooks.FirstOrDefault();
				}
				else if (matchingWebhooks.Count > 1)
				{
					return TypeReaderResult.FromError(CommandError.MultipleMatches, "Too many webhooks found with the same name.");
				}
			}
			return wh != null
				? TypeReaderResult.FromSuccess(wh)
				: TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching webhook.");
		}
	}
}
