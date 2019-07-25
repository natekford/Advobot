using Advobot.Gacha.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Advobot.Gacha.Database.Configuration
{
	public sealed class ImageConfig : IEntityTypeConfiguration<Image>
	{
		public void Configure(EntityTypeBuilder<Image> e)
		{
			e.ToTable("Image");
			e.HasKey(x => new { x.CharacterId, x.Url });
			e.HasOne(x => x.Character).WithMany(x => x.Images).IsRequired();
		}
	}
}
