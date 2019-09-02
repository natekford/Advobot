using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Gacha.Database;
using Advobot.Gacha.Interaction;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Services.Time;
using AdvorangesUtils;

using Discord;

namespace Advobot.Gacha.Displays
{
	public class SourceDisplay : PaginatedDisplay
	{
		private readonly IReadOnlyList<IReadOnlyCharacter> _Characters;
		private readonly IReadOnlySource _Source;

		public SourceDisplay(
			GachaDatabase db,
			ITime time,
			IInteractionManager interaction,
			int id,
			IReadOnlySource source,
			IReadOnlyList<IReadOnlyCharacter> characters)
			: base(db, time, interaction, id, characters.Count, GachaConstants.CharactersPerPage)
		{
			_Source = source;
			_Characters = characters;
		}

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
				Author = new EmbedAuthorBuilder
				{
					Name = "Placeholder Name",
					IconUrl = "https://cdn.discordapp.com/attachments/367092372636434443/597957769038921758/image0-4-1.jpg",
				},
				Footer = GeneratePaginationFooter(),
			}.Build();
		}
	}
}