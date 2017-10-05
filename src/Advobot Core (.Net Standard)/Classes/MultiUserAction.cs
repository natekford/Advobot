using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Classes
{
	public class MultiUserAction
	{
		private readonly IAdvobotCommandContext _Context;
		private IReadOnlyList<IGuildUser> _Users;

		public MultiUserAction(IAdvobotCommandContext context, IEnumerable<IGuildUser> users, bool bypass)
		{
			_Context = context;
			_Users = users.ToList().GetUpToAndIncludingMinNum(GetActions.GetMaxAmountOfUsersToGather(context.BotSettings, bypass));

			if (new Random().NextDouble() > .98)
			{
				ConsoleActions.WriteLine("Multi-user drifting!!");
			}
		}

		public async Task TakeRoleFromManyUsers(IRole role, ModerationReason reason)
		{
			await DoAction(Action.GiveRole, role, $"take the role `{role.FormatRole()}` from", $"took the role `{role.FormatRole()} from", reason);
		}
		public async Task GiveRoleToManyUsers(IRole role, ModerationReason reason)
		{
			await DoAction(Action.GiveRole, role, $"give the role `{role.FormatRole()}` to", $"gave the role `{role.FormatRole()} to", reason);
		}
		public async Task NicknameManyUsers(string replace, ModerationReason reason)
		{
			await DoAction(Action.Nickname, replace, "nickname", "nicknamed", reason);
		}
		public async Task MoveManyUsers(IVoiceChannel outputChannel, ModerationReason reason)
		{
			await DoAction(Action.Move, outputChannel, "move", "moved", reason);
		}

		private async Task DoAction(Action action, object obj, string presentTense, string pastTense, ModerationReason reason)
		{
			var msg = await MessageActions.SendMessage(_Context.Channel, $"Attempting to {presentTense} `{_Users.Count}` users.");
			for (int i = 0; i < _Users.Count; ++i)
			{
				if (i % 10 == 0)
				{
					var amtLeft = _Users.Count - i;
					var time = (int)(amtLeft * 1.2);
					await msg.ModifyAsync(x => x.Content = $"Attempting to {presentTense} `{amtLeft}` people. ETA on completion: `{time}`.");
				}

				switch (action)
				{
					case Action.GiveRole:
					{
						await RoleActions.GiveRoles(_Users[i], new[] { obj as IRole }, reason);
						continue;
					}
					case Action.TakeRole:
					{
						await RoleActions.TakeRoles(_Users[i], new[] { obj as IRole }, reason);
						continue;
					}
					case Action.Nickname:
					{
						await UserActions.ChangeNickname(_Users[i], obj as string, reason);
						continue;
					}
					case Action.Move:
					{
						await UserActions.MoveUser(_Users[i], obj as IVoiceChannel, reason);
						continue;
					}
				}
			}

			await MessageActions.DeleteMessage(msg);
			await MessageActions.MakeAndDeleteSecondaryMessage(_Context, $"Successfully {pastTense} `{_Users.Count}` users.");
		}

		private enum Action
		{
			GiveRole,
			TakeRole,
			Nickname,
			Move,
		}
	}
}
