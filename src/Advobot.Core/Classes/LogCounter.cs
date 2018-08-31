using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using AdvorangesUtils;

namespace Advobot.Classes
{
	/// <summary>
	/// Used in logging. Holds the name of what is being logged and the count.
	/// </summary>
	public sealed class LogCounter : INotifyPropertyChanged
	{
		/// <summary>
		/// The title of the log counter.
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// How many instances have been logged.
		/// </summary>
		public int Count => _Count;

		private int _Count;

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Creates an instance of <see cref="LogCounter"/>.
		/// </summary>
		/// <param name="name"></param>
		public LogCounter([CallerMemberName] string name = "")
		{
			Name = name.FormatTitle().Trim();
		}

		/// <summary>
		/// Add a specified amount to the counter.
		/// </summary>
		/// <param name="count"></param>
		public void Add(int count)
		{
			Interlocked.Add(ref _Count, count);
			NotifyPropertyChanged(nameof(Count));
		}
		/// <summary>
		/// Remove a specified amount from the counter.
		/// </summary>
		/// <param name="count"></param>
		public void Remove(int count)
		{
			Interlocked.Add(ref _Count, -count);
			NotifyPropertyChanged(nameof(Count));
		}
		/// <summary>
		/// Fires the property changed event.
		/// </summary>
		/// <param name="propertyName"></param>
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		/// <summary>
		/// Returns the title and count.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"**{Name}:** {Count}";
		}
	}
}