using Advobot.Gacha.Database;
using Advobot.Gacha.Interaction;
using Advobot.Gacha.Trading;
using Advobot.Services.Time;

using AdvorangesUtils;

using Discord;

using InteractionType = Advobot.Gacha.Interaction.InteractionType;

namespace Advobot.Gacha.Displays;

public class GiveDisplay : PaginatedDisplay
{
	private readonly IGuildUser _Giver;
	private readonly IGuildUser _Receiver;
	private readonly IReadOnlyList<Trade> _Trades;

	public GiveDisplay(
		GachaDatabase db,
		ITime time,
		IInteractionManager interaction,
		int id,
		IGuildUser giver,
		IGuildUser receiver,
		IReadOnlyList<Trade> trades)
		: base(db, time, interaction, id, trades.Count, GachaConstants.CharactersPerPage)
	{
		_Giver = giver;
		_Receiver = receiver;
		_Trades = trades;

		InteractionHandler.AddInteraction(InteractionType.Confirm);
		InteractionHandler.AddInteraction(InteractionType.Deny);
	}

	protected override Task<Embed> GenerateEmbedAsync()
		=> Task.FromResult(GenerateEmbed());

	protected override Task<string> GenerateTextAsync()
		=> Task.FromResult(GenerateText());

	private Embed GenerateEmbed()
	{
		var values = GetPageValues(_Trades);
		var description = values.Select(x => x.Character.Name).Join("\n");

		return new EmbedBuilder
		{
			Description = description,
			Author = new()
			{
				Name = $"{_Giver.Username} giving {_Receiver.Username}",
				IconUrl = _Giver.GetAvatarUrl(),
			},
			Footer = GeneratePaginationFooter(),
		}.Build();
	}

	private string GenerateText()
		=> $"{_Giver.Mention} giving {_Trades.Count} characters to {_Receiver.Mention}";
}

internal class ConfirmationDisplay;