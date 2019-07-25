using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Gacha.Displays
{
	public class SourceDisplay : PaginatedDisplay
	{
		private readonly Source _Source;

		public SourceDisplay(
			BaseSocketClient client,
			GachaDatabase db,
			Source source) : base(client, db, source.Characters.Count, Constants.CharactersPerPage)
		{
			_Source = source;
		}

		protected override Task<Embed> GenerateEmbedAsync()
			=> Task.FromResult(GenerateEmbed());
		protected override Task<string> GenerateTextAsync()
			=> Task.FromResult("");
		private Embed GenerateEmbed()
		{
			var values = GetPageValues(_Source.Characters);
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
