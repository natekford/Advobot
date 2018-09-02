using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ReactiveUI;

namespace Advobot.NetCoreUI.Classes.Controls
{
	public class UserListEditor : UserControl
	{
		public static readonly DirectProperty<UserListEditor, ObservableCollection<ulong>> UserListProperty =
			AvaloniaProperty.RegisterDirect<UserListEditor, ObservableCollection<ulong>>(
				nameof(UserList),
				o => o.UserList,
				(o, v) => o.UserList = v);
		public ObservableCollection<ulong> UserList
		{
			get => _DisplayList;
			set
			{
				//If null, no reason to bother creating a display list
				if ((_ActualList = value) == null)
				{
					SetAndRaise(UserListProperty, ref _DisplayList, value);
					return;
				}

				//If not null, need to create a display list since the original observable collection
				//will have an invalid invoking thread for some reason
				var displayList = new ObservableCollection<ulong>(_ActualList);
				_ActualList.CollectionChanged += (sender, e) =>
				{
					Dispatcher.UIThread.InvokeAsync(() =>
					{
						switch (e.Action)
						{
							case NotifyCollectionChangedAction.Add:
								foreach (ulong item in e.NewItems)
								{
									displayList.Add(item);
								}
								return;
							case NotifyCollectionChangedAction.Remove:
								foreach (ulong item in e.OldItems)
								{
									displayList.Remove(item);
								}
								return;
							default:
								throw new NotImplementedException();
						}
					});
				};
				SetAndRaise(UserListProperty, ref _DisplayList, displayList);
			}
		}
		private ObservableCollection<ulong> _DisplayList;
		private ObservableCollection<ulong> _ActualList;

		public static readonly DirectProperty<UserListEditor, ulong> CurrentIdProperty =
			AvaloniaProperty.RegisterDirect<UserListEditor, ulong>(
				nameof(CurrentId),
				o => o.CurrentId,
				(o, v) => o.CurrentId = v);
		public ulong CurrentId
		{
			get => _CurrentId;
			set => SetAndRaise(CurrentIdProperty, ref _CurrentId, value);
		}
		private ulong _CurrentId;

		private static readonly DirectProperty<UserListEditor, bool> CannotExecuteProperty =
			AvaloniaProperty.RegisterDirect<UserListEditor, bool>(
				nameof(CannotExecute),
				o => o.CannotExecute,
				(o, v) => o.CannotExecute = v);
		private bool CannotExecute
		{
			get => _CannotExecute;
			set => SetAndRaise(CannotExecuteProperty, ref _CannotExecute, value);
		}
		private bool _CannotExecute;

		private ICommand ModifyListCommand { get; }

		public UserListEditor()
		{
			ModifyListCommand = ReactiveCommand.Create<string>(x =>
			{
				//Actual has to go before display because display being modified modifies the displayed text
				//Or we can capture the value in a variable
				var id = CurrentId;
				if (!bool.Parse(x))
				{
					_ActualList.Remove(id);
					_DisplayList.Remove(id);
				}
				else if (!_ActualList.Contains(id))
				{
					_ActualList.Add(id);
					_DisplayList.Add(id);
				}
			}, this.WhenAnyValue(x => x.CannotExecute, x => !x));
			InitializeComponent();
		}

		private void InitializeComponent()
			=> AvaloniaXamlLoader.Load(this);
	}
}
