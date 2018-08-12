using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.TypeReaders;
using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Commands.Rules
{
	[Category(typeof(ModifyRuleCategories)), Group(nameof(ModifyRuleCategories)), TopLevelShortAlias(typeof(ModifyRuleCategories))]
	[Summary("Modifies the rule categories which hold rules.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	[SaveGuildSettings]
	public sealed class ModifyRuleCategories : AdvobotModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add([VerifyStringLength(Target.RuleCategory)] string name)
		{
			if (Context.GuildSettings.Rules.Categories.Keys.CaseInsContains(name))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"The category `{name}` already exists.")).CAF();
				return;
			}

			var pos = Context.GuildSettings.Rules.Categories.Count + 1;
			Context.GuildSettings.Rules.Categories.Add(name, new List<string>());
			var resp = $"Successfully created the category `{name}` at `{pos}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(ChangeName)), ShortAlias(nameof(ChangeName))]
		public async Task ChangeName([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category, [VerifyStringLength(Target.RuleCategory)] string newName)
		{
			var oldVal = Context.GuildSettings.Rules.Categories[category];
			Context.GuildSettings.Rules.Categories.Remove(category);
			Context.GuildSettings.Rules.Categories.Add(category, oldVal);
			var resp = $"Successfully changed the category `{category}` to `{newName}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category)
		{
			Context.GuildSettings.Rules.Categories.Remove(category);
			var resp = $"Successfully removed the category `{category}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(ModifyRules)), Group(nameof(ModifyRules)), TopLevelShortAlias(typeof(ModifyRules))]
	[Summary("Modifies the rules which are saved in the bot settings.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	[SaveGuildSettings]
	public sealed class ModifyRules : AdvobotModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category, [VerifyStringLength(Target.Rule)] string rule)
		{
			if (Context.GuildSettings.Rules.Categories[category].CaseInsContains(rule))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"The supplied rule already exists.")).CAF();
				return;
			}

			Context.GuildSettings.Rules.Categories[category].Add(rule);
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully added a rule in `{category}`.").CAF();
		}
		[Command(nameof(Insert)), ShortAlias(nameof(Insert))]
		public async Task Insert([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category, uint index, [VerifyStringLength(Target.Rule)] string rule)
		{
			var count = Context.GuildSettings.Rules.Categories[category].Count;
			Context.GuildSettings.Rules.Categories[category].Insert(Math.Min((int)Math.Min(index, int.MaxValue), count - 1), rule);
			var resp = $"Successfully removed the rule at index `{index}` in `{category}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category, uint index)
		{
			if (Context.GuildSettings.Rules.Categories[category].Count >= index)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"{index} is an invalid position to remove at.")).CAF();
				return;
			}
			Context.GuildSettings.Rules.Categories[category].RemoveAt((int)Math.Min(index, int.MaxValue));
			var resp = $"Successfully removed the rule at index `{index}` in `{category}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(PrintOutRules)), Group(nameof(PrintOutRules)), TopLevelShortAlias(typeof(PrintOutRules))]
	[Summary("Prints out the rules with given formatting options. " +
		"`Format` uses the `" + nameof(RuleFormat) + "` enum. " +
		"`TitleFormat` and `RuleFormat` use the `" + nameof(MarkDownFormat) + "` enum. " +
		"`FormatOptions` use the `" + nameof(RuleFormatOption) + "` enum.")]
	[OtherRequirement(Precondition.GenericPerms)]
	[DefaultEnabled(false)]
	public sealed class PrintOutRules : AdvobotModuleBase
	{
		[Command]
		public async Task Command([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category, [Optional, Remainder] NamedArguments<RuleFormatter> args)
		{
			RuleFormatter obj;
			if (args == null)
			{
				obj = new RuleFormatter();
			}
			else if (!args.TryCreateObject(new object[0], out obj, out var error))
			{
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			await Context.GuildSettings.Rules.SendCategoryAsync(obj, category, Context.Channel).CAF();
		}
		[Command]
		public async Task Command([Optional, Remainder] NamedArguments<RuleFormatter> args)
		{
			if (Context.GuildSettings.Rules.Categories.Count == 0)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("This guild has no rules set up.")).CAF();
				return;
			}

			RuleFormatter obj;
			if (args == null)
			{
				obj = new RuleFormatter();
			}
			else if (!args.TryCreateObject(new object[0], out obj, out var error))
			{
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			await Context.GuildSettings.Rules.SendAsync(obj, Context.Channel).CAF();
		}
	}
}
