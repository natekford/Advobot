using Advobot.Gacha.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Advobot.Gacha.Database.Configuration
{
	public sealed class UserConfig : IEntityTypeConfiguration<User>
	{
		public void Configure(EntityTypeBuilder<User> e)
		{
			e.ToTable("User");
			e.HasKey(x => new { x.GuildId, x.UserId });
			e.HasMany(x => x.Marriages)
				.WithOne(x => x.User)
				.OnDelete(DeleteBehavior.Cascade);
			e.HasMany(x => x.Wishlist)
				.WithOne(x => x.User)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
