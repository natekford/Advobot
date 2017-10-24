using Advobot.Core;
using Advobot.Core.Actions;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.Rules;
using Advobot.Core.Enums;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.Rules
{
	[Group(nameof(ModifyRuleCategories)), TopLevelShortAlias(typeof(ModifyRuleCategories))]
	[Summary("Modifies the rule categories which hold rules.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyRuleCategories : SavingModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add([VerifyStringLength(Target.RuleCategory)] string name)
		{
			if (Context.GuildSettings.Rules.Categories.Select(x => x.Name).CaseInsContains(name))
			{
				var error = new ErrorReason($"The category `{name}` already exists.");
				await MessageActions.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			var pos = Context.GuildSettings.Rules.Categories.Count + 1;
			Context.GuildSettings.Rules.AddCategory(new RuleCategory(name));
			var resp = $"Successfully created the category `{name}` at `{pos}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(ChangeName)), ShortAlias(nameof(ChangeName))]
		public async Task ChangeName(RuleCategory category, [VerifyStringLength(Target.RuleCategory)] string newName)
		{
			var oldName = category.Name;
			category.ChangeName(newName);
			var resp = $"Successfully changed the category `{oldName}` to `{newName}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(RuleCategory category)
		{
			Context.GuildSettings.Rules.RemoveCategory(category);
			var resp = $"Successfully removed the category `{category.Name}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyRules)), TopLevelShortAlias(typeof(ModifyRules))]
	[Summary("Modifies the rules which are saved in the bot settings.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyRules : SavingModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add(RuleCategory category, [VerifyStringLength(Target.Rule)] string rule)
		{
			if (Context.GuildSettings.Rules.Categories.SelectMany(x => x.Rules).Select(x => x.Text).CaseInsContains(rule))
			{
				var error = new ErrorReason($"The supplied rule already exists.");
				await MessageActions.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			category.AddRule(new Rule(rule));
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully added a rule in `{category.Name}`.").CAF();
		}
		[Command(nameof(ChangeText)), ShortAlias(nameof(ChangeText))]
		public async Task ChangeText(RuleCategory category, uint position, [VerifyStringLength(Target.Rule)] string newRule)
		{
			category.ChangeRule((int)position - 1, newRule);
			var resp = $"Successfully updated the rule at position `{position}` in `{category.Name}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(RuleCategory category, uint position)
		{
			category.RemoveRule((int)position - 1);
			var resp = $"Successfully removed the rule at position `{position}` in `{category.Name}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(PrintOutRules)), TopLevelShortAlias(typeof(PrintOutRules))]
	[Summary("Prints out the rules with given formatting options. " +
		"`Format` uses the `" + nameof(RuleFormat) + "` enum. " +
		"`TitleFormat` and `RuleFormat` use the `" + nameof(MarkDownFormat) + "` enum. " +
		"`FormatOptions` use the `" + nameof(RuleFormatOption) + "` enum.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(false)]
	public sealed class PrintOutRules : AdvobotModuleBase
	{
		[Command]
		public async Task Command(RuleCategory category, [Optional, Remainder] CustomArguments<RuleFormatter> formatter)
		{
			var obj = formatter?.CreateObject() ?? new RuleFormatter();
			var index = Array.IndexOf(Context.GuildSettings.Rules.Categories.ToArray(), category);
			obj.SetCategory(category, index);
			await obj.SendAsync(Context.Channel).CAF();
		}
		[Command]
		public async Task Command([Optional, Remainder] CustomArguments<RuleFormatter> formatter)
		{
			if (Context.GuildSettings.Rules.Categories.Count == 0)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("This guild has no rules set up.")).CAF();
				return;
			}

			var obj = formatter?.CreateObject() ?? new RuleFormatter();
			obj.SetRulesAndCategories(Context.GuildSettings.Rules);
			await obj.SendAsync(Context.Channel).CAF();
		}
	}
}
