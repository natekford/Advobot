using Advobot.Gacha.Models;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Image = Advobot.Gacha.Models.Image;

namespace Advobot.Gacha.Database
{
	public sealed class GachaDatabase : DbContext
	{
		public DbSet<Character> Characters { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<Marriage> Marriages { get; set; }
		public DbSet<Wish> Wishes { get; set; }
		public DbSet<Image> Images { get; set; }

		private readonly IBotDirectoryAccessor _DirectoryAccessor;

#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public GachaDatabase(IServiceProvider provider)
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
		{
			_DirectoryAccessor = provider.GetRequiredService<IBotDirectoryAccessor>();
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>(m =>
			{
				m.ToTable("User");
				m.HasKey(x => new { x.GuildId, x.UserId });
				m.HasMany(x => x.Marriages).WithOne(x => x.User).OnDelete(DeleteBehavior.Cascade);
				m.HasMany(x => x.Wishlist).WithOne(x => x.User).OnDelete(DeleteBehavior.Cascade);
			});

			modelBuilder.Entity<Character>(m =>
			{
				m.ToTable("Character");
				m.HasKey(x => x.CharacterId);
				m.HasMany(x => x.Images).WithOne(x => x.Character).OnDelete(DeleteBehavior.Cascade);
				m.HasMany(x => x.Marriages).WithOne(x => x.Character).OnDelete(DeleteBehavior.Cascade);
				m.HasMany(x => x.Wishlist).WithOne(x => x.Character).OnDelete(DeleteBehavior.Cascade);
				m.HasOne(x => x.Source).WithMany(x => x.Characters);
			});

			modelBuilder.Entity<Marriage>(m =>
			{
				m.ToTable("Marriage");
				m.HasKey(x => new { x.User.GuildId, x.Character.CharacterId });
				m.HasOne(x => x.Image).WithOne().OnDelete(DeleteBehavior.Cascade);
				m.HasOne(x => x.User).WithMany(x => x.Marriages).IsRequired();
				m.HasOne(x => x.Character).WithMany(x => x.Marriages).IsRequired();
			});

			modelBuilder.Entity<Wish>(m =>
			{
				m.ToTable("Wish");
				m.HasKey(x => new { x.User.GuildId, x.User.UserId, x.Character.CharacterId });
				m.HasOne(x => x.User).WithMany(x => x.Wishlist).IsRequired();
				m.HasOne(x => x.Character).WithMany(x => x.Wishlist).IsRequired();
			});

			modelBuilder.Entity<Image>(m =>
			{
				m.ToTable("Image");
				m.HasKey(x => new { x.Character.CharacterId, x.Url });
				m.HasOne(x => x.Character).WithMany(x => x.Images).IsRequired();
			});

			modelBuilder.Entity<Alias>(m =>
			{
				m.ToTable("Alias");
				m.HasKey(x => new { x.Character.CharacterId, x.Name });
				m.HasOne(x => x.Character).WithMany(x => x.Aliases);
			});

			modelBuilder.Entity<Source>(m =>
			{
				m.ToTable("Source");
				m.HasKey(x => x.SourceId);
				m.HasMany(x => x.Characters).WithOne(x => x.Source).IsRequired();
			});

			base.OnModelCreating(modelBuilder);
		}
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			var file = AdvobotUtils.ValidateDbPath(_DirectoryAccessor, "SQLite", "Gacha.db");
			var connectionStringBuilder = new SqliteConnectionStringBuilder
			{
				DataSource = file.FullName,
				Mode = SqliteOpenMode.ReadWriteCreate,
			};
			optionsBuilder.UseSqlite(connectionStringBuilder.ToString());

			base.OnConfiguring(optionsBuilder);
		}

		public async Task AddAndSaveAsync<T>(T value) where T : class
		{
			var set = Set<T>();
			await set.AddAsync(value).CAF();
			await SaveChangesAsync().CAF();
		}
		public async Task UpdateAsync<T>(T value) where T : class
		{
			var set = Set<T>();
			set.Update(value);
			await SaveChangesAsync().CAF();
		}
		public async Task UpdateAsync<T, TProperty>(
			T entity,
			Expression<Func<T, TProperty>> propertyExpression,
			TProperty value) where T : class
		{
			Attach(entity);
			Entry(entity).Property(propertyExpression).CurrentValue = value;
			await SaveChangesAsync().CAF();
		}

		public Task<Character> GetRandomCharacterAsync(IGuildUser user)
		{
			var untaken = Characters.Where(c => !Marriages.Any(m =>
				m.User.GuildId == user.GuildId &&
				m.User.UserId == user.Id &&
				m.Character.CharacterId == c.CharacterId)
			);
			var count = untaken.Count();
			var rng = new Random().Next(1, count + 1);
			return untaken.Skip(rng).FirstOrDefaultAsync();
		}
		public async Task<IReadOnlyList<Wish>> GetWishesAsync(IGuild guild, int id)
		{
			var filtered = Wishes.Where(x => x.User.GuildId == guild.Id && x.Character.CharacterId == id);
			return await filtered.ToArrayAsync().CAF();
		}

		public Task<User> GetUserAsync(IGuildUser user)
			=> Users.FindAsync(new { user.GuildId, user.Id });


		public Task<Marriage> GetMarriageAsync(IGuild guild, int id)
			=> Marriages.FindAsync(new { guild.Id, id });

		public Task<Character> GetCharacterAsync(int id)
			=> Characters.FindAsync(id);
	}
}
