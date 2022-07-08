﻿using Advobot.Quotes.Models;
using Advobot.SQLite;

using AdvorangesUtils;

using System.Data.SQLite;

namespace Advobot.Quotes.Database;

public static class RuleDatabaseUtils
{
	public static async Task<IReadOnlyDictionary<RuleCategory, IReadOnlyList<Rule>>> GetRuleDictionaryAsync(
		this RuleDatabase db,
		ulong guildId)
	{
		var categories = await db.GetCategoriesAsync(guildId).CAF();
		var rules = await db.GetRulesAsync(guildId).CAF();

		var dict = new Dictionary<RuleCategory, IReadOnlyList<Rule>>();
		foreach (var category in categories)
		{
			var list = new List<Rule>();
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

	public RuleDatabase(IConnectionString<RuleDatabase> conn) : base(conn)
	{
	}

	public Task<int> DeleteRuleAsync(Rule rule)
		=> ModifyAsync(DELETE_RULE, rule);

	public Task<int> DeleteRuleCategoryAsync(RuleCategory category)
	{
		return ModifyAsync(@"
				DELETE FROM RuleCategory
				WHERE GuildId = @GuildId AND Category = @Category;
				DELETE FROM Rule
				WHERE GuildId = @GuildId AND Category = @Category
			", category);
	}

	public Task<int> DeleteRulesAsync(IEnumerable<Rule> rules)
		=> BulkModifyAsync(DELETE_RULE, rules);

	public async Task<IReadOnlyList<RuleCategory>> GetCategoriesAsync(ulong guildId)
	{
		var param = new { GuildId = guildId.ToString(), };
		return await GetManyAsync<RuleCategory>(@"
				SELECT *
				FROM RuleCategory
				WHERE GuildId = @GuildId
				ORDER BY Category ASC
			", param).CAF();
	}

	public async Task<RuleCategory> GetCategoryAsync(ulong guildId, int category)
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

	public async Task<Rule?> GetRuleAsync(ulong guildId, int category, int position)
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

	public async Task<IReadOnlyList<Rule>> GetRulesAsync(ulong guildId)
	{
		var param = new { GuildId = guildId.ToString(), };
		return await GetManyAsync<Rule>(@"
				SELECT *
				FROM Rule
				WHERE GuildId = @GuildId
				ORDER BY Position ASC
			", param).CAF();
	}

	public async Task<IReadOnlyList<Rule>> GetRulesAsync(RuleCategory category)
	{
		return await GetManyAsync<Rule>(@"
				SELECT *
				FROM Rule
				WHERE GuildId = @GuildId AND Category = @Category
				ORDER BY Position ASC
			", category).CAF();
	}

	public Task<int> UpsertRuleAsync(Rule rule)
		=> ModifyAsync(UPSERT_RULE, rule);

	public Task<int> UpsertRuleCategoryAsync(RuleCategory category)
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

	public Task<int> UpsertRulesAsync(IEnumerable<Rule> rules)
		=> BulkModifyAsync(UPSERT_RULE, rules);
}