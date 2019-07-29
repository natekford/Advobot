using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attempts to find an <see cref="IWebhook"/> on a guild.
	/// </summary>
	[TypeReaderTargetType(typeof(IWebhook))]
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
			var webhooks = await context.Guild.GetWebhooksAsync().CAF();
			if (ulong.TryParse(input, out var id))
			{
				var webhook = webhooks.FirstOrDefault(x => x.Id == id);
				if (webhook != null)
				{
					return TypeReaderResult.FromSuccess(webhook);
				}
			}

			var matchingWebhooks = webhooks.Where(x => x.Name.CaseInsEquals(input)).ToArray();
			if (matchingWebhooks.Length == 1)
			{
				return TypeReaderResult.FromSuccess(matchingWebhooks[0]);
			}
			if (matchingWebhooks.Length > 1)
			{
				return TypeReaderResult.FromError(CommandError.MultipleMatches, "Too many webhooks found with the same name.");
			}
			return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Webhook not found.");
		}
	}
}
