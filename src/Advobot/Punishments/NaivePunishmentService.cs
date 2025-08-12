using Advobot.Services;

using Discord;

namespace Advobot.Punishments;

[Replacable]
internal sealed class NaivePunishmentService : IPunishmentService
{
	/// <inheritdoc />
	public Task PunishAsync(IPunishmentContext context, RequestOptions? options = null)
		=> context.ExecuteAsync(options);
}