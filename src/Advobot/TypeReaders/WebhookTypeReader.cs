using Advobot.Attributes;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.TypeReaders;

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
	public override async Task<TypeReaderResult> ReadAsync(
		ICommandContext context,
		string input,
		IServiceProvider services)
	{
		var webhooks = await context.Guild.GetWebhooksAsync().ConfigureAwait(false);

		if (ulong.TryParse(input, out var id)
			&& webhooks.FirstOrDefault(x => x.Id == id) is IWebhook wh)
		{
			return TypeReaderResult.FromSuccess(wh);
		}

		var matches = webhooks.Where(x => x.Name.CaseInsEquals(input)).ToArray();
		return TypeReaderUtils.SingleValidResult(matches, "webhooks", input);
	}
}