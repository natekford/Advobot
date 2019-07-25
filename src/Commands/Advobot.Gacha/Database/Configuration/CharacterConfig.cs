using Advobot.Gacha.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Advobot.Gacha.Database.Configuration
{
	public sealed class CharacterConfig : IEntityTypeConfiguration<Character>
	{
		public void Configure(EntityTypeBuilder<Character> e)
		{
			e.ToTable("Character");
			e.HasKey(x => x.CharacterId);
			e.Property(x => x.CharacterId)
				.ValueGeneratedOnAdd();
			e.HasMany(x => x.Images)
				.WithOne(x => x.Character)
				.HasForeignKey(x => x.CharacterId)
				.OnDelete(DeleteBehavior.Cascade);
			e.HasOne(x => x.Source)
				.WithMany(x => x.Characters)
				.HasForeignKey(x => x.SourceId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
