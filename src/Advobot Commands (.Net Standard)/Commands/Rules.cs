using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Interfaces;
using Discord.WebSocket;
using Advobot.Actions.Formatting;
using Discord.Commands;
using Advobot.Enums;
using Advobot.Classes.Rules;
using Advobot.Actions;

namespace Advobot.Commands.Rules
{
	[Group(nameof(ModifyRules)), TopLevelShortAlias(typeof(ModifyRules))]
	[Summary("Modifies the rules which are saved in the bot settings.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyRules : SavingModuleBase
	{
		[Group(nameof(Add)), ShortAlias(nameof(Add))]
		public sealed class Add : SavingModuleBase
		{
			[Command]
			public async Task Command([VerifyStringLength(Target.RuleCategory)] string name)
			{
				if (Context.GuildSettings.Rules.Categories.Select(x => x.Name).CaseInsContains(name))
				{
					var error = new ErrorReason($"The category `{name}` already exists.");
					await MessageActions.SendErrorMessageAsync(Context, error);
					return;
				}

				var pos = Context.GuildSettings.Rules.Categories.Count + 1;
				Context.GuildSettings.Rules.AddCategory(new RuleCategory(name));
				var resp = $"Successfully created the category `{name}` at `{pos}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command]
			public async Task Command(RuleCategory category, [VerifyStringLength(Target.Rule)] string rule)
			{
				if (Context.GuildSettings.Rules.Categories.SelectMany(x => x.Rules).Select(x => x.Text).CaseInsContains(rule))
				{
					var error = new ErrorReason($"The supplied rule already exists.");
					await MessageActions.SendErrorMessageAsync(Context, error);
					return;
				}

				category.AddRule(new Rule(rule));
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully added a rule in `{category.Name}`.");
			}
		}
		[Group(nameof(Update)), ShortAlias(nameof(Update))]
		public sealed class Update : SavingModuleBase
		{
			[Command]
			public async Task Command(RuleCategory category, [VerifyStringLength(Target.RuleCategory)] string newName)
			{
				var oldName = category.Name;
				category.ChangeName(newName);
				var resp = $"Successfully changed the category `{oldName}` to `{newName}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command]
			public async Task Command(RuleCategory category, uint position, [VerifyStringLength(Target.Rule)] string newRule)
			{
				category.ChangeRule((int)position - 1, newRule);
				var resp = $"Successfully updated the rule at position `{position}` in `{category.Name}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp);
			}
		}
		[Group(nameof(Remove)), ShortAlias(nameof(Remove))]
		public sealed class Remove : SavingModuleBase
		{
			[Command]
			public async Task Command(RuleCategory category)
			{
				Context.GuildSettings.Rules.RemoveCategory(category);
				var resp = $"Successfully removed the category `{category.Name}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command]
			public async Task Command(RuleCategory category, uint position)
			{
				category.RemoveRule((int)position - 1);
				var resp = $"Successfully removed the rule at position `{position}` in `{category.Name}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp);
			}
		}
	}

	[Group(nameof(PrintOutRules)), TopLevelShortAlias(typeof(PrintOutRules))]
	[Summary("Prints out the rules with given formatting options.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(false)]
	public sealed class PrintOutRules : AdvobotModuleBase
	{
		[Command]
		public async Task Command(RuleCategory category, [Remainder] CustomArguments<RuleFormatter> formatter)
		{
			var obj = formatter.CreateObject();
			var index = Array.IndexOf(Context.GuildSettings.Rules.Categories.ToArray(), category);
			obj.SetCategory(category, index);
			await obj.SendAsync(Context.Channel).CAF();
		}
		[Command]
		public async Task Command([Remainder] CustomArguments<RuleFormatter> formatter)
		{
			if (Context.GuildSettings.Rules.Categories.Count == 0)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("This guild has no rules set up.")).CAF();
				return;
			}

			var obj = formatter.CreateObject();
			obj.SetRulesAndCategories(Context.GuildSettings.Rules);
			await obj.SendAsync(Context.Channel).CAF();
		}
	}
}
