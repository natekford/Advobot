using Advobot.Gacha.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Image = Advobot.Gacha.Models.Image;

namespace Advobot.Gacha.Database
{
	extern alias notnetstandard;

	public sealed class GachaContext : DbContext
	{
		private static readonly Assembly _CurrentAssembly = Assembly.GetExecutingAssembly();

		public DbSet<Character> Characters { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<Marriage> Marriages { get; set; }
		public DbSet<Wish> Wishes { get; set; }
		public DbSet<Image> Images { get; set; }
		public DbSet<Alias> Aliases { get; set; }
		public DbSet<Source> Sources { get; set; }

#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public GachaContext(DbContextOptions options) : base(options) { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfigurationsFromAssembly(_CurrentAssembly);
			base.OnModelCreating(modelBuilder);
		}
	}
}
