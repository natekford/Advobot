using Advobot.Gacha.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Advobot.Gacha.Database.Configuration
{
	public sealed class AliasConfig : IEntityTypeConfiguration<Alias>
	{
		public void Configure(EntityTypeBuilder<Alias> e)
		{
			e.ToTable("Alias");
			e.HasKey(x => new { x.CharacterId, x.Name });
			e.HasOne(x => x.Character).WithMany(x => x.Aliases);
		}
	}
}
