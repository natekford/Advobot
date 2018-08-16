using System.Windows;

namespace Advobot.NetFrameworkUI.Classes
{
	/// <summary>
	/// Arguments for modifying a user list.
	/// </summary>
	public class UserListModificationEventArgs : RoutedEventArgs
	{
		/// <summary>
		/// Whether to add or remove the value.
		/// </summary>
		public bool Add { get; }
		/// <summary>
		/// The supplied text.
		/// </summary>
		public ulong Value { get; }

		/// <summary>
		/// Creates an instance of <see cref="UserListModificationEventArgs"/>.
		/// </summary>
		/// <param name="routedEvent"></param>
		/// <param name="add"></param>
		/// <param name="value"></param>
		public UserListModificationEventArgs(RoutedEvent routedEvent, bool add, ulong value) : base(routedEvent)
		{
			Add = add;
			Value = value;
		}
	}
}