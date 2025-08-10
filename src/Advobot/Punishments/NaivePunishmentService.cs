using Advobot.Services;

namespace Advobot.Punishments;

[Replacable]
internal sealed class NaivePunishmentService : IPunishmentService
{
	/// <inheritdoc />
	public Task HandleAsync(IPunishmentContext context)
		=> context.ExecuteAsync();
}