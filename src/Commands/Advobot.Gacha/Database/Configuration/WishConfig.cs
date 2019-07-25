using Advobot.Gacha.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Advobot.Gacha.Database.Configuration
{
	public sealed class WishConfig : IEntityTypeConfiguration<Wish>
	{
		public void Configure(EntityTypeBuilder<Wish> e)
		{
			e.ToTable("Wish");
			e.HasKey(x => new { x.GuildId, x.UserId, x.CharacterId });
			e.HasOne(x => x.Character).WithMany().OnDelete(DeleteBehavior.Restrict);
			e.HasOne(x => x.User).WithMany(x => x.Wishlist).IsRequired();
		}
	}
}
