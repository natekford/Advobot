using Discord;

namespace Advobot.Tests.Fakes.Discord;

public sealed class FakeRole : FakeSnowflake, IRole
{
	private readonly FakeGuild _Guild;
	public Color Color { get; set; }
	public Emoji Emoji { get; set; }
	public RoleFlags Flags => throw new NotImplementedException();
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
		Position = guild.FakeRoles.Count;
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

		Color = args.Color.GetValueOrDefault(Color);
		IsHoisted = args.Hoist.GetValueOrDefault(IsHoisted);
		IsMentionable = args.Mentionable.GetValueOrDefault(IsMentionable);
		Name = args.Name.GetValueOrDefault(Name);
		Permissions = args.Permissions.GetValueOrDefault(Permissions);
		Position = args.Position.GetValueOrDefault(Position);

		return Task.CompletedTask;
	}
}