using Advobot.Gacha.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Advobot.Gacha.Database.Configuration
{
	public sealed class MarriageConfig : IEntityTypeConfiguration<Marriage>
	{
		public void Configure(EntityTypeBuilder<Marriage> e)
		{
			e.ToTable("Marriage");
			e.HasKey(x => new { x.GuildId, x.CharacterId });
			e.HasOne(x => x.Image).WithMany().OnDelete(DeleteBehavior.Restrict);
			e.HasOne(x => x.Character).WithMany().OnDelete(DeleteBehavior.Restrict);
			e.HasOne(x => x.User).WithMany(x => x.Marriages).IsRequired();
		}
	}
}
