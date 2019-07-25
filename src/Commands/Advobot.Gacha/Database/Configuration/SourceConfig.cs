using Advobot.Gacha.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Advobot.Gacha.Database.Configuration
{
	public sealed class SourceConfig : IEntityTypeConfiguration<Source>
	{
		public void Configure(EntityTypeBuilder<Source> e)
		{
			e.ToTable("Source");
			e.HasKey(x => x.SourceId);
			e.Property(x => x.SourceId).ValueGeneratedOnAdd();
			e.HasMany(x => x.Characters).WithOne(x => x.Source).IsRequired();
		}
	}
}
