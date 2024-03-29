﻿using AdvorangesUtils;

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Advobot.Services.LogCounters;

/// <summary>
/// Used in logging. Holds the name of what is being logged and the count.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="LogCounter"/>.
/// </remarks>
/// <param name="caller"></param>
internal sealed class LogCounter([CallerMemberName] string caller = "") : ILogCounter, INotifyPropertyChanged
{
	private int _Count;

	/// <summary>
	/// How many instances have been logged.
	/// </summary>
	public int Count => _Count;

	/// <summary>
	/// The title of the log counter.
	/// </summary>
	public string Name { get; } = caller;

	/// <inheritdoc />
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Add a specified amount to the counter.
	/// </summary>
	/// <param name="count"></param>
	public void Add(int count)
	{
		if (count == 0)
		{
			return;
		}

		Interlocked.Add(ref _Count, count);
		NotifyPropertyChanged(nameof(Count));
	}

	/// <summary>
	/// Remove a specified amount from the counter.
	/// </summary>
	/// <param name="count"></param>
	public void Remove(int count)
	{
		if (count == 0)
		{
			return;
		}

		Interlocked.Add(ref _Count, -count);
		NotifyPropertyChanged(nameof(Count));
	}

	/// <summary>
	/// Returns the title and count.
	/// </summary>
	/// <returns></returns>
	public override string ToString()
		=> $"**{Name.FormatTitle().Trim()}:** {Count}";

	/// <summary>
	/// Fires the property changed event.
	/// </summary>
	/// <param name="caller"></param>
	private void NotifyPropertyChanged([CallerMemberName] string caller = "")
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
}