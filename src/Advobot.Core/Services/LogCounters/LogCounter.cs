using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

using AdvorangesUtils;

namespace Advobot.Services.LogCounters
{
	/// <summary>
	/// Used in logging. Holds the name of what is being logged and the count.
	/// </summary>
	internal sealed class LogCounter : ILogCounter, INotifyPropertyChanged
	{
		private int _Count;

		/// <summary>
		/// How many instances have been logged.
		/// </summary>
		public int Count => _Count;

		/// <summary>
		/// The title of the log counter.
		/// </summary>
		public string Name { get; }

		/// <inheritdoc />
		public event PropertyChangedEventHandler? PropertyChanged;

		/// <summary>
		/// Creates an instance of <see cref="LogCounter"/>.
		/// </summary>
		/// <param name="caller"></param>
		public LogCounter([CallerMemberName] string caller = "")
		{
			Name = caller;
		}

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
}