using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace Advobot.NetCoreUI.Classes.Controls
{
	public class UserListEditor : UserControl
	{
		public static readonly DirectProperty<UserListEditor, IList<ulong>> UserListProperty =
			AvaloniaProperty.RegisterDirect<UserListEditor, IList<ulong>>(nameof(UserList), o => o.UserList, (o, v) => o.UserList = v);
		public IList<ulong> UserList
		{
			get => _UserList;
			set => SetAndRaise(UserListProperty, ref _UserList, value);
		}
		private IList<ulong> _UserList;

		private ulong AddId { get; set; }
		private ulong RemoveId { get; set; }

		private ICommand ModifyList { get; }

		public UserListEditor()
		{
			ModifyList = ReactiveCommand.Create<string>(x =>
			{
				var errs = DataValidationErrors.GetErrors(this);
				if (!bool.Parse(x))
				{
					UserList.Remove(RemoveId);
				}
				else if (!UserList.Contains(AddId))
				{
					UserList.Add(AddId);
				}
			});
			InitializeComponent();
		}

		private void InitializeComponent()
			=> AvaloniaXamlLoader.Load(this);
	}
}
