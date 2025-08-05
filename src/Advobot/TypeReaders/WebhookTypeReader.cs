using Advobot.Attributes;
using Advobot.Utilities;

using AdvorangesUtils;

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
		var whs = await context.Guild.GetWebhooksAsync().CAF();
		if (ulong.TryParse(input, out var id) && whs.TryGetFirst(x => x.Id == id, out var wh))
		{
			return TypeReaderResult.FromSuccess(wh);
		}

		var matches = whs.Where(x => x.Name.CaseInsEquals(input)).ToArray();
		return TypeReaderUtils.SingleValidResult(matches, "webhooks", input);
	}
}