using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Does an action on all the input users until either no more users remain or the cancel token has been canceled.
	/// </summary>
	public class MultiUserAction
	{
		private static ConcurrentDictionary<ulong, CancellationTokenSource> _CancelTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();

		private CancellationTokenSource _CancelToken;
		private ICommandContext _Context;
		private ITimersService _Timers;
		private List<IGuildUser> _Users;

		public MultiUserAction(ICommandContext context, ITimersService timers, IEnumerable<IGuildUser> users)
		{
			_CancelToken = new CancellationTokenSource();
			_CancelTokens.AddOrUpdate(context.Guild.Id, _CancelToken, (oldKey, oldValue) =>
			{
				oldValue.Cancel();
				return _CancelToken;
			});
			_Context = context;
			_Timers = timers;
			_Users = users.ToList();
		}

		public async Task TakeRolesAsync(IRole role, ModerationReason reason)
		{
			var presentTense = $"take the role `{role.Format()}` from";
			var pastTense = $"took the role `{role.Format()} from";
			await DoActionAsync(nameof(TakeRolesAsync), role, presentTense, pastTense, reason).CAF();
		}
		public async Task GiveRolesAsync(IRole role, ModerationReason reason)
		{
			var presentTense = $"give the role `{role.Format()}` to";
			var pastTense = $"gave the role `{role.Format()} to";
			await DoActionAsync(nameof(GiveRolesAsync), role, presentTense, pastTense, reason).CAF();
		}
		public async Task ModifyNicknamesAsync(string replace, ModerationReason reason)
		{
			var presentTense = "nickname";
			var pastTense = "nicknamed";
			await DoActionAsync(nameof(ModifyNicknamesAsync), replace, presentTense, pastTense, reason).CAF();
		}
		public async Task MoveUsersAsync(IVoiceChannel outputChannel, ModerationReason reason)
		{
			var presentTense = "move";
			var pastTense = "moved";
			await DoActionAsync(nameof(MoveUsersAsync), outputChannel, presentTense, pastTense, reason).CAF();
		}

		private async Task DoActionAsync(string action, object obj, string presentTense, string pastTense, ModerationReason reason)
		{
			var text = $"Attempting to {presentTense} `{_Users.Count}` users.";
			var msg = await MessageUtils.SendMessageAsync(_Context.Channel, text).CAF();

			var successCount = 0;
			for (var i = 0; i < _Users.Count; ++i)
			{
				if (_CancelToken.IsCancellationRequested)
				{
					break;
				}

				if (i % 10 == 0)
				{
					var amtLeft = _Users.Count - i;
					var time = (int)(amtLeft * 1.2);
					var newText = $"Attempting to {presentTense} `{amtLeft}` people. ETA on completion: `{time}`.";
					await msg.ModifyAsync(x => x.Content = newText).CAF();
				}

				++successCount;
				switch (action)
				{
					case nameof(GiveRolesAsync):
						await RoleUtils.GiveRolesAsync(_Users[i], new[] { obj as IRole }, reason).CAF();
						continue;
					case nameof(TakeRolesAsync):
						await RoleUtils.TakeRolesAsync(_Users[i], new[] { obj as IRole }, reason).CAF();
						continue;
					case nameof(ModifyNicknamesAsync):
						await UserUtils.ChangeNicknameAsync(_Users[i], obj as string, reason).CAF();
						continue;
					case nameof(MoveUsersAsync):
						await UserUtils.MoveUserAsync(_Users[i], obj as IVoiceChannel, reason).CAF();
						continue;
				}
			}

			await MessageUtils.DeleteMessageAsync(msg, new ModerationReason("multi user action")).CAF();
			var response = $"Successfully {pastTense} `{successCount}` users.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(_Timers, _Context.Channel, _Context.Message, response).CAF();
		}
	}
}
