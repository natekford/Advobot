
using Advobot.Gacha.Metadata;
using Advobot.Gacha.Models;
using Advobot.Gacha.Trading;

namespace Advobot.Gacha.Database
{
	public interface IGachaDatabase
	{
		CloseIds CharacterIds { get; }
		CloseIds SourceIds { get; }

		Task<int> AddCharacterAsync(Character character);

		Task<int> AddCharactersAsync(IEnumerable<Character> characters);

		Task<int> AddClaimAsync(Claim claim);

		Task<int> AddClaimsAsync(IEnumerable<Claim> claims);

		Task<int> AddImageAsync(Image image);

		Task<int> AddSourceAsync(Source source);

		Task<int> AddSourcesAsync(IEnumerable<Source> sources);

		Task<int> AddUserAsync(User user);

		Task<int> AddUsersAsync(IEnumerable<User> users);

		Task<int> AddWishAsync(Wish wish);

		Task<Character> GetCharacterAsync(long id);

		Task<CharacterMetadata> GetCharacterMetadataAsync(Character character);

		Task<IReadOnlyList<Character>> GetCharactersAsync();

		Task<IReadOnlyList<Character>> GetCharactersAsync(IEnumerable<long> ids);

		Task<IReadOnlyList<Character>> GetCharactersAsync(Source source);

		Task<Claim> GetClaimAsync(User user, Character character);

		Task<Claim> GetClaimAsync(ulong guildId, Character character);

		Task<IReadOnlyList<Claim>> GetClaimsAsync(User user);

		Task<IReadOnlyList<Claim>> GetClaimsAsync(ulong guildId);

		Task<IReadOnlyList<Image>> GetImagesAsync(Character character);

		Task<Source> GetSourceAsync(long sourceId);

		Task<IReadOnlyList<Source>> GetSourcesAsync(IEnumerable<long> ids);

		Task<Character> GetUnclaimedCharacter(ulong guildId);

		Task<User> GetUserAsync(ulong guildId, ulong userId);

		Task<IReadOnlyList<Wish>> GetWishesAsync(User user);

		Task<IReadOnlyList<Wish>> GetWishesAsync(ulong guildId);

		Task<IReadOnlyList<Wish>> GetWishesAsync(ulong guildId, Character character);

		Task<int> TradeAsync(IEnumerable<Trade> trades);

		Task UpdateClaimImageUrlAsync(Claim claim, string? url);
	}
}