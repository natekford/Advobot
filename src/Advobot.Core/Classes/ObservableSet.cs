using System.Collections.ObjectModel;

namespace Advobot.Classes
{
	/// <summary>
	/// Observable collection but only allows one of each matching item in.
	/// Gotten from https://stackoverflow.com/a/527000.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class ObservableSet<T> : ObservableCollection<T>
	{
		/// <inheritdoc />
		protected override void InsertItem(int index, T item)
		{
			if (Contains(item))
			{
				return;
			}

			base.InsertItem(index, item);
		}
		/// <inheritdoc />
		protected override void SetItem(int index, T item)
		{
			var i = IndexOf(item);
			if (i >= 0 && i != index)
			{
				return;
			}
			base.SetItem(index, item);
		}
	}
}
