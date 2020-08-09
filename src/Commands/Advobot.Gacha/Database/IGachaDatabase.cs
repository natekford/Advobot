using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Gacha.Metadata;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Trading;

namespace Advobot.Gacha.Database
{
	public interface IGachaDatabase
	{
		CloseIds CharacterIds { get; }
		CloseIds SourceIds { get; }

		Task<int> AddCharacterAsync(IReadOnlyCharacter character);

		Task<int> AddCharactersAsync(IEnumerable<IReadOnlyCharacter> characters);

		Task<int> AddClaimAsync(IReadOnlyClaim claim);

		Task<int> AddClaimsAsync(IEnumerable<IReadOnlyClaim> claims);

		Task<int> AddImageAsync(IReadOnlyImage image);

		Task<int> AddSourceAsync(IReadOnlySource source);

		Task<int> AddSourcesAsync(IEnumerable<IReadOnlySource> sources);

		Task<int> AddUserAsync(IReadOnlyUser user);

		Task<int> AddUsersAsync(IEnumerable<IReadOnlyUser> users);

		Task<int> AddWishAsync(IReadOnlyWish wish);

		Task<IReadOnlyCharacter> GetCharacterAsync(long id);

		Task<CharacterMetadata> GetCharacterMetadataAsync(IReadOnlyCharacter character);

		Task<IReadOnlyList<IReadOnlyCharacter>> GetCharactersAsync();

		Task<IReadOnlyList<IReadOnlyCharacter>> GetCharactersAsync(IEnumerable<long> ids);

		Task<IReadOnlyList<IReadOnlyCharacter>> GetCharactersAsync(IReadOnlySource source);

		Task<IReadOnlyClaim> GetClaimAsync(IReadOnlyUser user, IReadOnlyCharacter character);

		Task<IReadOnlyClaim> GetClaimAsync(ulong guildId, IReadOnlyCharacter character);

		Task<IReadOnlyList<IReadOnlyClaim>> GetClaimsAsync(IReadOnlyUser user);

		Task<IReadOnlyList<IReadOnlyClaim>> GetClaimsAsync(ulong guildId);

		Task<IReadOnlyList<IReadOnlyImage>> GetImagesAsync(IReadOnlyCharacter character);

		Task<IReadOnlySource> GetSourceAsync(long sourceId);

		Task<IReadOnlyList<IReadOnlySource>> GetSourcesAsync(IEnumerable<long> ids);

		Task<IReadOnlyCharacter> GetUnclaimedCharacter(ulong guildId);

		Task<IReadOnlyUser> GetUserAsync(ulong guildId, ulong userId);

		Task<IReadOnlyList<IReadOnlyWish>> GetWishesAsync(IReadOnlyUser user);

		Task<IReadOnlyList<IReadOnlyWish>> GetWishesAsync(ulong guildId);

		Task<IReadOnlyList<IReadOnlyWish>> GetWishesAsync(ulong guildId, IReadOnlyCharacter character);

		Task<int> TradeAsync(IEnumerable<ITrade> trades);

		Task UpdateClaimImageUrlAsync(IReadOnlyClaim claim, string? url);
	}
}