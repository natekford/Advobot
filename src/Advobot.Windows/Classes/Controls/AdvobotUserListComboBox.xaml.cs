using System.Windows;

namespace Advobot.Windows.Classes.Controls
{
	/// <summary>
	/// Interaction logic for AdvobotUserListCombobox.xaml
	/// </summary>
	public partial class AdvobotUserListCombobox : AdvobotComboBox
	{
		/// <summary>
		/// Event manager for modifying a user list.
		/// </summary>
		public static readonly RoutedEvent UserListModifiedEvent = EventManager.RegisterRoutedEvent(nameof(UserListModified), RoutingStrategy.Bubble, typeof(UserListModificationEventHandler), typeof(AdvobotUserListCombobox));
		/// <summary>
		/// Event for modifying a user list.
		/// </summary>
		public event UserListModificationEventHandler UserListModified
		{
			add => AddHandler(UserListModifiedEvent, value);
			remove => RemoveHandler(UserListModifiedEvent, value);
		}

		/// <summary>
		/// Creates an instance of <see cref="AdvobotUserListCombobox"/>.
		/// </summary>
		public AdvobotUserListCombobox()
		{
			InitializeComponent();
		}

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			RaiseEvent(new UserListModificationEventArgs(UserListModifiedEvent, true, ulong.Parse(Text)));
			Text = null;
		}
		private void RemoveButton_Click(object sender, RoutedEventArgs e)
		{
			RaiseEvent(new UserListModificationEventArgs(UserListModifiedEvent, false, ulong.Parse(Text)));
			Text = null;
		}
	}
}