using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Advobot.NetFrameworkUI.Classes
{
	/// <summary>
	/// Holds an object for use in XAML binding.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Holder<T> : INotifyPropertyChanged
	{
		/// <summary>
		/// The held object.
		/// </summary>
		public T HeldObject
		{
			get => _HeldObject;
			internal set
			{
				_HeldObject = value;
				NotifyPropertyChanged();
			}
		}

		private T _HeldObject;

		/// <summary>
		/// Notifies that the held object has been updated.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] string name = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		/// <summary>
		/// Implicitly converts the holder to its held value.
		/// </summary>
		/// <param name="holder"></param>
		public static implicit operator T(Holder<T> holder)
		{
			return holder.HeldObject;
		}
	}
}
