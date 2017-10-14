using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Advobot.Classes
{
	/// <summary>
	/// Handles deleted message collection for <see cref="Modules.Log.MyLogModule.OnMessageDeleted(Cacheable{IMessage, ulong}, ISocketMessageChannel)"/>.
	/// </summary>
	public class MessageDeletion
	{
		public CancellationTokenSource CancelToken { get; private set; }
		private List<IMessage> _Messages = new List<IMessage>();

		public void SetCancelToken(CancellationTokenSource cancelToken)
		{
			CancelToken = cancelToken;
		}
		public List<IMessage> GetList()
		{
			return _Messages.ToList();
		}
		public void SetList(List<IMessage> newList)
		{
			_Messages = newList;
		}
		public void AddToList(IMessage item)
		{
			_Messages.Add(item);
		}
		public void ClearList()
		{
			_Messages.Clear();
		}
	}
}