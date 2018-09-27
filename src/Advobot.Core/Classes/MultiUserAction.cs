using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes
{
#warning fully implement this into a module
	/// <summary>
	/// Does an action on all the input users until either no more users remain or the cancel token has been canceled.
	/// </summary>
	public sealed class MultiUserActionModule : AdvobotModuleBase
	{
		private static ConcurrentDictionary<ulong, CancellationTokenSource> _CancelTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();

		private readonly CancellationTokenSource _CancelToken;
		private readonly SocketTextChannel _Channel;
		private readonly IUserMessage _Message;
		private readonly List<SocketGuildUser> _Users;

		/// <summary>
		/// Creates an instance of multi user action and cancels all previous instances.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="users"></param>
		public MultiUserActionModule(ICommandContext context, IEnumerable<SocketGuildUser> users)
		{
			_CancelToken = new CancellationTokenSource();
			_CancelTokens.AddOrUpdate(context.Guild.Id, _CancelToken, (oldKey, oldValue) =>
			{
				oldValue.Cancel();
				return _CancelToken;
			});
			_Channel = (SocketTextChannel)context.Channel;
			_Message = context.Message;
			_Users = users.ToList();
		}

		/// <summary>
		/// Take a role from multiple users.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task TakeRolesAsync(SocketRole role, RequestOptions options)
		{
			var r = role.Format();
			await DoActionAsync(nameof(TakeRolesAsync), role, $"take the role `{r}` from", $"took the role `{r}` from", options).CAF();
		}
		/// <summary>
		/// Give a role to multiple users.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task GiveRolesAsync(SocketRole role, RequestOptions options)
		{
			var r = role.Format();
			await DoActionAsync(nameof(GiveRolesAsync), role, $"give the role `{r}` to", $"gave the role `{r}` to", options).CAF();
		}
		/// <summary>
		/// Modify the nickname of multiple users.
		/// </summary>
		/// <param name="replace"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task ModifyNicknamesAsync(string replace, RequestOptions options)
			=> await DoActionAsync(nameof(ModifyNicknamesAsync), replace, "nickname", "nicknamed", options).CAF();
		/// <summary>
		/// Move multiple users.
		/// </summary>
		/// <param name="outputChannel"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task MoveUsersAsync(SocketVoiceChannel outputChannel, RequestOptions options)
			=> await DoActionAsync(nameof(MoveUsersAsync), outputChannel, "move", "moved", options).CAF();

#warning redo
		private async Task DoActionAsync(string action, object obj, string presentTense, string pastTense, RequestOptions options)
		{
			var text = $"Attempting to {presentTense} `{_Users.Count}` users.";
			var msg = await MessageUtils.SendMessageAsync(_Channel, text).CAF();

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
					var newText = $"Attempting to {presentTense} `{amtLeft}` people. ETA on completion: `{time}` seconds.";
					await msg.ModifyAsync(x => x.Content = newText).CAF();
				}

				++successCount;
				var user = _Users[i];
				await user.ModifyAsync(x =>
				{
					switch (action)
					{
						case nameof(GiveRolesAsync):
							x.RoleIds = Optional.Create(x.RoleIds.Value.Concat(new[] { ((IRole)obj).Id }).Distinct());
							return;
						case nameof(TakeRolesAsync):
							x.RoleIds = Optional.Create(x.RoleIds.Value.Except(new[] { ((IRole)obj).Id }));
							return;
						case nameof(ModifyNicknamesAsync):
							x.Nickname = obj as string ?? user.Username;
							return;
						case nameof(MoveUsersAsync):
							x.Channel = Optional.Create((IVoiceChannel)obj);
							return;
					}
				}, options);
			}

			await MessageUtils.DeleteMessageAsync(msg, options).CAF();
			await MessageUtils.SendMessageAsync(_Channel, $"Successfully {pastTense} `{successCount}` users.").CAF();
		}
	}
}
