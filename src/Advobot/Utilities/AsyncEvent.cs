﻿using System.Collections.Immutable;

namespace Advobot.Utilities;

//Taken from: https://github.com/discord-net/Discord.Net/blob/322d46e47b47e44f8b62b1ddcdcf39280cac6771/src/Discord.Net.Core/Utils/AsyncEvent.cs
internal static class EventExtensions
{
	public static async Task InvokeAsync(this AsyncEvent<Func<Task>> eventHandler)
	{
		var subscribers = eventHandler.Subscriptions;
		for (var i = 0; i < subscribers.Count; ++i)
		{
			await subscribers[i].Invoke().ConfigureAwait(false);
		}
	}

	public static async Task InvokeAsync<T>(this AsyncEvent<Func<T, Task>> eventHandler, T arg)
	{
		var subscribers = eventHandler.Subscriptions;
		for (var i = 0; i < subscribers.Count; ++i)
		{
			await subscribers[i].Invoke(arg).ConfigureAwait(false);
		}
	}

	public static async Task InvokeAsync<T1, T2>(this AsyncEvent<Func<T1, T2, Task>> eventHandler, T1 arg1, T2 arg2)
	{
		var subscribers = eventHandler.Subscriptions;
		for (var i = 0; i < subscribers.Count; ++i)
		{
			await subscribers[i].Invoke(arg1, arg2).ConfigureAwait(false);
		}
	}

	public static async Task InvokeAsync<T1, T2, T3>(this AsyncEvent<Func<T1, T2, T3, Task>> eventHandler, T1 arg1, T2 arg2, T3 arg3)
	{
		var subscribers = eventHandler.Subscriptions;
		for (var i = 0; i < subscribers.Count; ++i)
		{
			await subscribers[i].Invoke(arg1, arg2, arg3).ConfigureAwait(false);
		}
	}

	public static async Task InvokeAsync<T1, T2, T3, T4>(this AsyncEvent<Func<T1, T2, T3, T4, Task>> eventHandler, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		var subscribers = eventHandler.Subscriptions;
		for (var i = 0; i < subscribers.Count; ++i)
		{
			await subscribers[i].Invoke(arg1, arg2, arg3, arg4).ConfigureAwait(false);
		}
	}

	public static async Task InvokeAsync<T1, T2, T3, T4, T5>(this AsyncEvent<Func<T1, T2, T3, T4, T5, Task>> eventHandler, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		var subscribers = eventHandler.Subscriptions;
		for (var i = 0; i < subscribers.Count; ++i)
		{
			await subscribers[i].Invoke(arg1, arg2, arg3, arg4, arg5).ConfigureAwait(false);
		}
	}
}

internal class AsyncEvent<T> where T : class
{
	private readonly object _SubLock = new();
	private ImmutableArray<T> _Subscriptions;
	public bool HasSubscribers => _Subscriptions.Length != 0;
	public IReadOnlyList<T> Subscriptions => _Subscriptions;

	public AsyncEvent()
	{
		_Subscriptions = [];
	}

	public void Add(T subscriber)
	{
		ArgumentNullException.ThrowIfNull(subscriber);
		lock (_SubLock)
		{
			_Subscriptions = _Subscriptions.Add(subscriber);
		}
	}

	public void Remove(T subscriber)
	{
		ArgumentNullException.ThrowIfNull(subscriber);
		lock (_SubLock)
		{
			_Subscriptions = _Subscriptions.Remove(subscriber);
		}
	}
}