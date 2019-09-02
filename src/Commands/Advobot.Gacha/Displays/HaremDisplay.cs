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
	/// <summary>
	/// Displays a list of all the characters someone has claimed.
	/// </summary>
	public class HaremDisplay : PaginatedDisplay
	{
		private readonly IReadOnlyCollection<long> _Claims;
		private readonly IReadOnlyClaim? _Primary;

		public HaremDisplay(
			GachaDatabase db,
			ITime time,
			IInteractionManager interaction,
			int id,
			IReadOnlyCollection<IReadOnlyClaim> claims)
			: base(db, time, interaction, id, claims.Count, GachaConstants.CharactersPerPage)
		{
			_Claims = claims.Select(x => x.CharacterId).ToArray();
			_Primary = claims.FirstOrDefault();

			foreach (var marriage in claims)
			{
				if (marriage.IsPrimaryClaim)
				{
					_Primary = marriage;
				}
			}
		}

		protected override async Task<Embed> GenerateEmbedAsync()
		{
			var values = GetPageValues(_Claims);
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