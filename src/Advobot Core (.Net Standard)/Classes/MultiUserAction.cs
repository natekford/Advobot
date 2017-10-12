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

		public async Task TakeRoleFromManyUsersAsync(IRole role, ModerationReason reason)
		{
			await DoActionAsync(Action.GiveRole, role, $"take the role `{role.FormatRole()}` from", $"took the role `{role.FormatRole()} from", reason);
		}
		public async Task GiveRoleToManyUsersAsync(IRole role, ModerationReason reason)
		{
			await DoActionAsync(Action.GiveRole, role, $"give the role `{role.FormatRole()}` to", $"gave the role `{role.FormatRole()} to", reason);
		}
		public async Task NicknameManyUsersAsync(string replace, ModerationReason reason)
		{
			await DoActionAsync(Action.Nickname, replace, "nickname", "nicknamed", reason);
		}
		public async Task MoveManyUsersAsync(IVoiceChannel outputChannel, ModerationReason reason)
		{
			await DoActionAsync(Action.Move, outputChannel, "move", "moved", reason);
		}

		private async Task DoActionAsync(Action action, object obj, string presentTense, string pastTense, ModerationReason reason)
		{
			var msg = await MessageActions.SendMessageAsync(_Context.Channel, $"Attempting to {presentTense} `{_Users.Count}` users.");
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
						await RoleActions.GiveRolesAsync(_Users[i], new[] { obj as IRole }, reason);
						continue;
					}
					case Action.TakeRole:
					{
						await RoleActions.TakeRolesAsync(_Users[i], new[] { obj as IRole }, reason);
						continue;
					}
					case Action.Nickname:
					{
						await UserActions.ChangeNicknameAsync(_Users[i], obj as string, reason);
						continue;
					}
					case Action.Move:
					{
						await UserActions.MoveUserAsync(_Users[i], obj as IVoiceChannel, reason);
						continue;
					}
				}
			}

			await MessageActions.DeleteMessageAsync(msg);
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(_Context, $"Successfully {pastTense} `{_Users.Count}` users.");
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
