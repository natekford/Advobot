#define TWO_LISTS
//Because without having the actual source list from the settings and a seperate list for the UI
//when the settings list gets modified it attempts to modify the UI from not the UI thread

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
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
#if TWO_LISTS
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
							case NotifyCollectionChangedAction.Reset:
								displayList.Clear();
								return;
							default:
								throw new NotImplementedException();
						}
					});
				};
				SetAndRaise(UserListProperty, ref _DisplayList, displayList);
			}
		}
		private ObservableCollection<ulong> _DisplayList = new ObservableCollection<ulong>();
		private ObservableCollection<ulong> _ActualList = new ObservableCollection<ulong>();
#else
		public ObservableCollection<ulong> UserList
		{
			get => _UserList;
			set => SetAndRaise(UserListProperty, ref _UserList, value);
		}
		private ObservableCollection<ulong> _UserList;
#endif

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

		public static readonly DirectProperty<UserListEditor, bool> HasErrorProperty =
			AvaloniaProperty.RegisterDirect<UserListEditor, bool>(
				nameof(HasError),
				o => o.HasError,
				(o, v) => o.HasError = v,
				defaultBindingMode: BindingMode.OneWayToSource);
		public bool HasError
		{
			get => _HasError;
			private set => SetAndRaise(HasErrorProperty, ref _HasError, value);
		}
		private bool _HasError;

		public ICommand ModifyListCommand { get; }

		public UserListEditor()
		{
			ModifyListCommand = ReactiveCommand.Create<string>(x =>
			{
				//Actual has to go before display because display being modified modifies the displayed text
				//Or we can capture the value in a variable
				var id = CurrentId;
#if TWO_LISTS
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
#else
				if (!bool.Parse(x))
				{
					_UserList.Remove(id);
				}
				else if (!_UserList.Contains(id))
				{
					_UserList.Add(id);
				}
#endif
			}, this.WhenAnyValue(x => x.HasError, x => !x));
			InitializeComponent();
		}

		private void InitializeComponent()
			=> AvaloniaXamlLoader.Load(this);
	}
}