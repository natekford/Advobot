using AdvorangesUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Utilities
{
	/// <summary>
	/// Handles 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <returns></returns>
	public delegate Task AsyncEventHandler<T>(object sender, T e) where T : EventArgs;

	/// <summary>
	/// Extensions to make <see cref="AsyncEventHandler{T}"/> work.
	/// </summary>
	/// <remarks>
	/// Copied from https://stackoverflow.com/a/35280607.
	/// </remarks>
	public static class AsyncEventHandlerExtensions
	{
		/// <summary>
		/// Gets all of the items subscribed to this event.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="handler"></param>
		/// <returns></returns>
		public static IEnumerable<AsyncEventHandler<T>> GetHandlers<T>(this AsyncEventHandler<T> handler) where T : EventArgs
			=> handler.GetInvocationList().Cast<AsyncEventHandler<T>>();
		/// <summary>
		/// Invokes every 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		public static async Task InvokeAsync<T>(this AsyncEventHandler<T> handler, object sender, T e) where T : EventArgs
		{
			//Invoke sequentially instead of all at once just in case
			foreach (var h in handler.GetHandlers())
			{
				await h.Invoke(sender, e).CAF();
			}
		}
	}
}
