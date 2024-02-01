using Advobot.Gacha.Database;
using Advobot.Gacha.Metadata;
using Advobot.Gacha.Models;
using Advobot.Gacha.Trading;

using AdvorangesUtils;

namespace Advobot.Tests.Commands.Gacha;

public sealed class FakeGachaDatabase : IGachaDatabase
{
	private readonly List<Character> _Characters = [];
	private readonly List<Source> _Sources = [];
	public CloseIds CharacterIds { get; } = new()
	{
		IncludeWhenContains = false,
		MaxAllowedCloseness = 2,
	};
	public CloseIds SourceIds { get; } = new()
	{
		IncludeWhenContains = false,
		MaxAllowedCloseness = 2,
	};

	public Task<int> AddCharacterAsync(Character character)
	{
		CharacterIds.Add(character.CharacterId, character.Name);
		_Characters.Add(character);
		return Task.FromResult(1);
	}

	public async Task<int> AddCharactersAsync(IEnumerable<Character> characters)
	{
		var count = 0;
		foreach (var character in characters)
		{
			await AddCharacterAsync(character).CAF();
			++count;
		}
		return count;
	}

	public Task<int> AddClaimAsync(Claim claim)
		=> throw new NotImplementedException();

	public Task<int> AddClaimsAsync(IEnumerable<Claim> claims)
		=> throw new NotImplementedException();

	public Task<int> AddImageAsync(Image image)
		=> throw new NotImplementedException();

	public Task<int> AddSourceAsync(Source source)
	{
		SourceIds.Add(source.SourceId, source.Name);
		_Sources.Add(source);
		return Task.FromResult(1);
	}

	public async Task<int> AddSourcesAsync(IEnumerable<Source> sources)
	{
		var count = 0;
		foreach (var source in sources)
		{
			await AddSourceAsync(source).CAF();
			++count;
		}
		return count;
	}

	public Task<int> AddUserAsync(User user)
		=> throw new NotImplementedException();

	public Task<int> AddUsersAsync(IEnumerable<User> users)
		=> throw new NotImplementedException();

	public Task<int> AddWishAsync(Wish wish)
		=> throw new NotImplementedException();

	public Task<Character> GetCharacterAsync(long id)
		=> throw new NotImplementedException();

	public Task<CharacterMetadata> GetCharacterMetadataAsync(Character character)
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<Character>> GetCharactersAsync()
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<Character>> GetCharactersAsync(IEnumerable<long> ids)
		=> Task.FromResult<IReadOnlyList<Character>>(_Characters.Where(x => ids.Contains(x.CharacterId)).ToArray());

	public Task<IReadOnlyList<Character>> GetCharactersAsync(Source source)
		=> throw new NotImplementedException();

	public Task<Claim?> GetClaimAsync(User user, Character character)
		=> throw new NotImplementedException();

	public Task<Claim?> GetClaimAsync(ulong guildId, Character character)
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<Claim>> GetClaimsAsync(User user)
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<Claim>> GetClaimsAsync(ulong guildId)
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<Image>> GetImagesAsync(Character character)
		=> throw new NotImplementedException();

	public Task<Source> GetSourceAsync(long sourceId)
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<Source>> GetSourcesAsync(IEnumerable<long> ids)
		=> Task.FromResult<IReadOnlyList<Source>>(_Sources.Where(x => ids.Contains(x.SourceId)).ToArray());

	public Task<Character?> GetUnclaimedCharacter(ulong guildId)
		=> throw new NotImplementedException();

	public Task<User> GetUserAsync(ulong guildId, ulong userId)
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<Wish>> GetWishesAsync(User user)
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<Wish>> GetWishesAsync(ulong guildId)
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<Wish>> GetWishesAsync(ulong guildId, Character character)
		=> throw new NotImplementedException();

	public Task<int> TradeAsync(IEnumerable<Trade> trades)
		=> throw new NotImplementedException();

	public Task UpdateClaimImageUrlAsync(Claim claim, string? url)
		=> throw new NotImplementedException();
}