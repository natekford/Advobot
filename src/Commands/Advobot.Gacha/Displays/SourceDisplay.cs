using Advobot.Gacha.Database;
using Advobot.Gacha.Interaction;
using Advobot.Gacha.Models;
using Advobot.Services.Time;

using AdvorangesUtils;

using Discord;

namespace Advobot.Gacha.Displays;

public class SourceDisplay(
	IGachaDatabase db,
	ITime time,
	IInteractionManager interaction,
	int id,
	Source source,
	IReadOnlyList<Character> characters) : PaginatedDisplay(db, time, interaction, id, characters.Count, GachaConstants.CharactersPerPage)
{
	private readonly IReadOnlyList<Character> _Characters = characters;
	private readonly Source _Source = source;

	protected override Task<Embed> GenerateEmbedAsync()
		=> Task.FromResult(GenerateEmbed());

	protected override Task<string> GenerateTextAsync()
		=> Task.FromResult("");

	private Embed GenerateEmbed()
	{
		var values = GetPageValues(_Characters);
		var description = values.Select(x => x.Name).Join("\n");

		return new EmbedBuilder
		{
			Description = description,
			ThumbnailUrl = _Source.ThumbnailUrl,
			Author = new()
			{
				Name = "Placeholder Name",
				IconUrl = "https://cdn.discordapp.com/attachments/367092372636434443/597957769038921758/image0-4-1.jpg",
			},
			Footer = GeneratePaginationFooter(),
		}.Build();
	}
}