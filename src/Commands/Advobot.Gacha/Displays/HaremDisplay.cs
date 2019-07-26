using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Gacha.Displays
{
	/// <summary>
	/// Displays a list of all the characters someone has claimed.
	/// </summary>
	public class HaremDisplay : PaginatedDisplay
	{
		private readonly IReadOnlyCollection<long> _Marriages;
		private readonly Claim? _Primary;

		public HaremDisplay(
			BaseSocketClient client,
			GachaDatabase db,
			IReadOnlyCollection<Claim> marriages) : base(client, db, marriages.Count, Constants.CharactersPerPage)
		{
			_Marriages = marriages.Select(x => x.CharacterId).ToArray();
			_Primary = marriages.FirstOrDefault();

			foreach (var marriage in marriages)
			{
				if (marriage.IsPrimaryMarriage)
				{
					_Primary = marriage;
				}
			}
		}

		protected override async Task<Embed> GenerateEmbedAsync()
		{
			var values = GetPageValues(_Marriages);
			var characters = await Database.GetCharactersAsync(values).CAF();
			var description = characters.Select(x => x.Name).Join("\n");

			return new EmbedBuilder
			{
				Description = description,
				ThumbnailUrl = _Primary?.ImageUrl,
				Author = new EmbedAuthorBuilder
				{
					Name = "Placeholder Name",
					IconUrl = "https://cdn.discordapp.com/attachments/367092372636434443/597957769038921758/image0-4-1.jpg",
				},
				Footer = GeneratePaginationFooter(),
			}.Build();
		}
		protected override Task<string> GenerateTextAsync()
			=> Task.FromResult("");
	}
}
