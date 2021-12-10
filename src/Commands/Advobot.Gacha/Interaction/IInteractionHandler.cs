namespace Advobot.Gacha.Interaction;

public interface IInteractionHandler
{
	public IList<IInteraction> Interactions { get; }

	public void AddInteraction(InteractionType interaction);

	public Task StartAsync();

	public Task StopAsync();
}