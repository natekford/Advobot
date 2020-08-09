using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Gacha.Database;
using Advobot.Gacha.Metadata;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Trading;

using AdvorangesUtils;

namespace Advobot.Tests.Commands.Gacha.Fakes
{
	public sealed class FakeGachaDatabase : IGachaDatabase
	{
		private readonly List<IReadOnlyCharacter> _Characters = new List<IReadOnlyCharacter>();
		private readonly List<IReadOnlySource> _Sources = new List<IReadOnlySource>();
		public CloseIds CharacterIds { get; } = new CloseIds
		{
			IncludeWhenContains = false,
			MaxAllowedCloseness = 2,
		};
		public CloseIds SourceIds { get; } = new CloseIds
		{
			IncludeWhenContains = false,
			MaxAllowedCloseness = 2,
		};

		public Task<int> AddCharacterAsync(IReadOnlyCharacter character)
		{
			CharacterIds.Add(character.CharacterId, character.Name);
			_Characters.Add(character);
			return Task.FromResult(1);
		}

		public async Task<int> AddCharactersAsync(IEnumerable<IReadOnlyCharacter> characters)
		{
			var count = 0;
			foreach (var character in characters)
			{
				await AddCharacterAsync(character).CAF();
				++count;
			}
			return count;
		}

		public Task<int> AddClaimAsync(IReadOnlyClaim claim) => throw new NotImplementedException();

		public Task<int> AddClaimsAsync(IEnumerable<IReadOnlyClaim> claims) => throw new NotImplementedException();

		public Task<int> AddImageAsync(IReadOnlyImage image) => throw new NotImplementedException();

		public Task<int> AddSourceAsync(IReadOnlySource source)
		{
			SourceIds.Add(source.SourceId, source.Name);
			_Sources.Add(source);
			return Task.FromResult(1);
		}

		public async Task<int> AddSourcesAsync(IEnumerable<IReadOnlySource> sources)
		{
			var count = 0;
			foreach (var source in sources)
			{
				await AddSourceAsync(source).CAF();
				++count;
			}
			return count;
		}

		public Task<int> AddUserAsync(IReadOnlyUser user) => throw new NotImplementedException();

		public Task<int> AddUsersAsync(IEnumerable<IReadOnlyUser> users) => throw new NotImplementedException();

		public Task<int> AddWishAsync(IReadOnlyWish wish) => throw new NotImplementedException();

		public Task<IReadOnlyCharacter> GetCharacterAsync(long id) => throw new NotImplementedException();

		public Task<CharacterMetadata> GetCharacterMetadataAsync(IReadOnlyCharacter character) => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlyCharacter>> GetCharactersAsync() => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlyCharacter>> GetCharactersAsync(IEnumerable<long> ids)
			=> Task.FromResult<IReadOnlyList<IReadOnlyCharacter>>(_Characters.Where(x => ids.Contains(x.CharacterId)).ToArray());

		public Task<IReadOnlyList<IReadOnlyCharacter>> GetCharactersAsync(IReadOnlySource source) => throw new NotImplementedException();

		public Task<IReadOnlyClaim> GetClaimAsync(IReadOnlyUser user, IReadOnlyCharacter character) => throw new NotImplementedException();

		public Task<IReadOnlyClaim> GetClaimAsync(ulong guildId, IReadOnlyCharacter character) => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlyClaim>> GetClaimsAsync(IReadOnlyUser user) => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlyClaim>> GetClaimsAsync(ulong guildId) => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlyImage>> GetImagesAsync(IReadOnlyCharacter character) => throw new NotImplementedException();

		public Task<IReadOnlySource> GetSourceAsync(long sourceId) => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlySource>> GetSourcesAsync(IEnumerable<long> ids)
			=> Task.FromResult<IReadOnlyList<IReadOnlySource>>(_Sources.Where(x => ids.Contains(x.SourceId)).ToArray());

		public Task<IReadOnlyCharacter> GetUnclaimedCharacter(ulong guildId) => throw new NotImplementedException();

		public Task<IReadOnlyUser> GetUserAsync(ulong guildId, ulong userId) => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlyWish>> GetWishesAsync(IReadOnlyUser user) => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlyWish>> GetWishesAsync(ulong guildId) => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlyWish>> GetWishesAsync(ulong guildId, IReadOnlyCharacter character) => throw new NotImplementedException();

		public Task<int> TradeAsync(IEnumerable<ITrade> trades) => throw new NotImplementedException();

		public Task UpdateClaimImageUrlAsync(IReadOnlyClaim claim, string? url) => throw new NotImplementedException();
	}
}