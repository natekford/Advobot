using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

using Advobot.Quotes.Models;
using Advobot.Quotes.ReadOnlyModels;
using Advobot.SQLite;

using AdvorangesUtils;

namespace Advobot.Quotes.Database
{
	public static class RuleDatabaseUtils
	{
		public static async Task<IReadOnlyDictionary<IReadOnlyRuleCategory, IReadOnlyList<IReadOnlyRule>>> GetRuleDictionaryAsync(
			this RuleDatabase db,
			ulong guildId)
		{
			var categories = await db.GetCategoriesAsync(guildId).CAF();
			var rules = await db.GetRulesAsync(guildId).CAF();

			var dict = new Dictionary<IReadOnlyRuleCategory, IReadOnlyList<IReadOnlyRule>>();
			foreach (var category in categories)
			{
				var list = new List<IReadOnlyRule>();
				dict.Add(category, list);

				foreach (var rule in rules)
				{
					if (rule.Category == category.Category)
					{
						list.Add(rule);
					}
				}
			}
			return dict;
		}
	}

	public sealed class RuleDatabase : DatabaseBase<SQLiteConnection>
	{
		private const string DELETE_RULE = @"
			DELETE FROM Rule
			WHERE GuildId = @GuildId AND Category = @Category AND Position = @Position
		";
		private const string UPSERT_RULE = @"
			INSERT OR IGNORE INTO Rule
			( GuildId, Value, Category, Position )
			VALUES
			( @GuildId, @Value, @Category, @Position )
			UPDATE Rule
			SET
				Category = @Category,
				Position = @Position,
				Value = @Value
			WHERE GuildId = @GuildId AND Category = @Category AND Position = @Position
		";

		public RuleDatabase(IConnectionStringFor<RuleDatabase> conn) : base(conn)
		{
		}

		public Task<int> DeleteRuleAsync(IReadOnlyRule rule)
			=> ModifyAsync(DELETE_RULE, rule);

		public Task<int> DeleteRuleCategoryAsync(IReadOnlyRuleCategory category)
		{
			return ModifyAsync(@"
				DELETE FROM RuleCategory
				WHERE GuildId = @GuildId AND Category = @Category;
				DELETE FROM Rule
				WHERE GuildId = @GuildId AND Category = @Category
			", category);
		}

		public Task<int> DeleteRulesAsync(IEnumerable<IReadOnlyRule> rules)
			=> BulkModifyAsync(DELETE_RULE, rules);

		public async Task<IReadOnlyList<IReadOnlyRuleCategory>> GetCategoriesAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<RuleCategory>(@"
				SELECT *
				FROM RuleCategory
				WHERE GuildId = @GuildId
				ORDER BY Category ASC
			", param).CAF();
		}

		public async Task<IReadOnlyRuleCategory> GetCategoryAsync(ulong guildId, int category)
		{
			var param = new
			{
				GuildId = guildId.ToString(),
				Category = category,
			};
			return await GetOneAsync<RuleCategory>(@"
				SELECT *
				FROM RuleCategory
				WHERE GuildId = @GuildId AND Category = @Category
			", param).CAF();
		}

		public async Task<IReadOnlyRule?> GetRuleAsync(ulong guildId, int category, int position)
		{
			var param = new
			{
				GuildId = guildId.ToString(),
				Category = category,
				Position = position,
			};
			return await GetOneAsync<Rule>(@"
				SELECT *
				FROM Rule
				WHERE GuildId = @GuildId AND Category = @Category AND Position = @Position
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyRule>> GetRulesAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<Rule>(@"
				SELECT *
				FROM Rule
				WHERE GuildId = @GuildId
				ORDER BY Position ASC
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyRule>> GetRulesAsync(IReadOnlyRuleCategory category)
		{
			return await GetManyAsync<Rule>(@"
				SELECT *
				FROM Rule
				WHERE GuildId = @GuildId AND Category = @Category
				ORDER BY Position ASC
			", category).CAF();
		}

		public Task<int> UpsertRuleAsync(IReadOnlyRule rule)
			=> ModifyAsync(UPSERT_RULE, rule);

		public Task<int> UpsertRuleCategoryAsync(IReadOnlyRuleCategory category)
		{
			return ModifyAsync(@"
				INSERT OR IGNORE INTO RuleCategory
				( GuildId, Value, Category )
				VALUES
				( @GuildId, @Value, @Category )
				UPDATE RuleCategory
				SET
					Value = @Value
				WHERE GuildId = @GuildId AND Category = @Category
			", category);
		}

		public Task<int> UpsertRulesAsync(IEnumerable<IReadOnlyRule> rules)
			=> BulkModifyAsync(UPSERT_RULE, rules);
	}
}