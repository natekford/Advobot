using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Classes
{
	public class MultiUserAction
	{
		private static ConcurrentDictionary<ulong, CancellationTokenSource> _CancelTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();

		private readonly CancellationTokenSource _CancelToken;
		private readonly IAdvobotCommandContext _Context;
		private readonly IReadOnlyList<IGuildUser> _Users;

		public MultiUserAction(IAdvobotCommandContext context, IEnumerable<IGuildUser> users, bool bypass)
		{
			_CancelToken = new CancellationTokenSource();
			_CancelTokens.AddOrUpdate(context.Guild.Id, _CancelToken, (oldKey, oldValue) =>
			{
				oldValue.Cancel();
				return _CancelToken;
			});
			_Context = context;
			_Users = users.ToList().GetUpToAndIncludingMinNum(GetActions.GetMaxAmountOfUsersToGather(context.BotSettings, bypass));

			if (new Random().NextDouble() > .98)
			{
				ConsoleActions.WriteLine("Multi-user drifting!!");
			}
		}

		public async Task TakeRoleFromManyUsersAsync(IRole role, ModerationReason reason)
		{
			var presentTense = $"take the role `{role.FormatRole()}` from";
			var pastTense = $"took the role `{role.FormatRole()} from";
			await DoActionAsync(Action.GiveRole, role, presentTense, pastTense, reason);
		}
		public async Task GiveRoleToManyUsersAsync(IRole role, ModerationReason reason)
		{
			var presentTense = $"give the role `{role.FormatRole()}` to";
			var pastTense = $"gave the role `{role.FormatRole()} to";
			await DoActionAsync(Action.GiveRole, role, presentTense, pastTense, reason);
		}
		public async Task NicknameManyUsersAsync(string replace, ModerationReason reason)
		{
			var presentTense = "nickname";
			var pastTense = "nicknamed";
			await DoActionAsync(Action.Nickname, replace, presentTense, pastTense, reason);
		}
		public async Task MoveManyUsersAsync(IVoiceChannel outputChannel, ModerationReason reason)
		{
			var presentTense = "move";
			var pastTense = "moved";
			await DoActionAsync(Action.Move, outputChannel, presentTense, pastTense, reason);
		}

		private async Task DoActionAsync(Action action, object obj, string presentTense, string pastTense, ModerationReason reason)
		{
			var msg = await MessageActions.SendMessageAsync(_Context.Channel, $"Attempting to {presentTense} `{_Users.Count}` users.");

			var successCount = 0;
			for (int i = 0; i < _Users.Count; ++i)
			{
				if (_CancelToken.IsCancellationRequested)
				{
					break;
				}
				else if (i % 10 == 0)
				{
					var amtLeft = _Users.Count - i;
					var time = (int)(amtLeft * 1.2);
					await msg.ModifyAsync(x => x.Content = $"Attempting to {presentTense} `{amtLeft}` people. ETA on completion: `{time}`.");
				}

				++successCount;
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
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(_Context, $"Successfully {pastTense} `{successCount}` users.");
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
