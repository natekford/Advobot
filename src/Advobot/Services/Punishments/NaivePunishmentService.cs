using Advobot.Punishments;
using Advobot.Services;

using Discord;

namespace Advobot.Services.Punishments;

[Replacable]
internal sealed class NaivePunishmentService : IPunishmentService
{
	/// <inheritdoc />
	public Task PunishAsync(IPunishment context, RequestOptions? options = null)
		=> context.ExecuteAsync(options);
}