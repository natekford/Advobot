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
			public async Task Command(string name)
			{
				var category = new RuleCategory(name);
				var pos = Context.GuildSettings.Rules.Categories.Count + 1;
				Context.GuildSettings.Rules.AddOrUpdateCategory(pos, category);
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully created a category at `{pos}`.");
			}
			[Command]
			public async Task Command(RuleCategory category, string rule)
			{

			}
		}
		[Group(nameof(Update)), ShortAlias(nameof(Update))]
		public sealed class Update : SavingModuleBase
		{
			[Command]
			public async Task Command(RuleCategory category, string newName)
			{
				var oldName = category.Name;
				category.ChangeName(newName);
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully changed the category `{oldName}` to `{newName}`.");
			}
			[Command]
			public async Task Command(RuleCategory category, int rulePosition, string newRule)
			{

			}
		}
		[Group(nameof(Remove)), ShortAlias(nameof(Remove))]
		public sealed class Remove : SavingModuleBase
		{
			[Command]
			public async Task Command(string name)
			{
				var category = new RuleCategory(name);
				var pos = Context.GuildSettings.Rules.Categories.Count + 1;
				Context.GuildSettings.Rules.AddOrUpdateCategory(pos, category);
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully created a category at `{pos}`.");
			}
			[Command]
			public async Task Command(RuleCategory category, string rule)
			{

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
			var text = formatter.CreateObject().FormatRuleCategory(category);
			await MessageActions.SendMessageAsync(Context.Channel, text);
		}
		[Command]
		public async Task Command([Remainder] CustomArguments<RuleFormatter> formatter)
		{
			if (Context.GuildSettings.Rules.Categories.Count == 0)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("This guild has no rules set up."));
				return;
			}

			var text = formatter.CreateObject().FormatRules(Context.GuildSettings.Rules);
			await MessageActions.SendMessageAsync(Context.Channel, text);
		}
	}
}
