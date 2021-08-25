
using Advobot.Gacha.Database;
using Advobot.Gacha.Interaction;
using Advobot.Gacha.Models;
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
		private readonly Claim? _Primary;

		public HaremDisplay(
			IGachaDatabase db,
			ITime time,
			IInteractionManager interaction,
			int id,
			IReadOnlyCollection<Claim> claims)
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
				Author = new()
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