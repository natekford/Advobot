using Discord;

namespace Advobot.Tests.Fakes.Discord
{
	public sealed class FakeRole : FakeSnowflake, IRole
	{
		private readonly FakeGuild _Guild;
		public Color Color { get; set; }
		public Emoji Emoji { get; set; }
		public IGuild Guild => _Guild;
		public string Icon { get; set; }
		public bool IsHoisted { get; set; }
		public bool IsManaged { get; set; }
		public bool IsMentionable { get; set; }
		public string Mention => $"<@&{Id}>";
		public string Name { get; set; }
		public GuildPermissions Permissions { get; set; }
		public int Position { get; set; }
		public RoleTags Tags { get; set; }

		public FakeRole(FakeGuild guild)
		{
			_Guild = guild;
			guild.FakeRoles.Add(this);
		}

		public int CompareTo(IRole? other)
			=> throw new NotImplementedException();

		public Task DeleteAsync(RequestOptions? options = null)
		{
			_Guild.FakeRoles.Remove(this);
			return Task.CompletedTask;
		}

		public string GetIconUrl()
			=> throw new NotImplementedException();

		public Task ModifyAsync(Action<RoleProperties> func, RequestOptions? options = null)
		{
			var args = new RoleProperties();
			func(args);

			Color = args.Color.GetValueOrDefault();
			IsHoisted = args.Hoist.GetValueOrDefault();
			IsMentionable = args.Mentionable.GetValueOrDefault();
			Name = args.Name.GetValueOrDefault();
			Permissions = args.Permissions.GetValueOrDefault();
			Position = args.Position.GetValueOrDefault();

			return Task.CompletedTask;
		}
	}
}