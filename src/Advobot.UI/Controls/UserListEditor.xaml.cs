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

namespace Advobot.UI.Controls
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
				if ((_UserList = value) == null)
				{
					SetAndRaise(UserListProperty, ref _DisplayList, value);
					return;
				}

				//If not null, need to create a display list since the original observable collection
				//will have an invalid invoking thread for some reason
				var displayList = new ObservableCollection<ulong>(_UserList);
				_UserList.CollectionChanged += (sender, e) =>
				{
					var action = e.Action switch
					{
						NotifyCollectionChangedAction.Add => (() =>
						{
							foreach (var item in e.NewItems ?? Array.Empty<object>())
							{
								if (item is ulong id)
								{
									displayList.Add(id);
								}
							}
						}),
						NotifyCollectionChangedAction.Remove => (() =>
						{
							foreach (var item in e.OldItems ?? Array.Empty<object>())
							{
								if (item is ulong id)
								{
									displayList.Remove(id);
								}
							}
						}),
						NotifyCollectionChangedAction.Reset => (Action)displayList.Clear,
						_ => throw new ArgumentOutOfRangeException(nameof(e), "Invalid collection changed action."),
					};
					Dispatcher.UIThread.InvokeAsync(action);
				};
				SetAndRaise(UserListProperty, ref _DisplayList, displayList);
			}
		}
		private ObservableCollection<ulong> _DisplayList = new();
#else
		public ObservableCollection<ulong> UserList
		{
			get => _UserList;
			set => SetAndRaise(UserListProperty, ref _UserList, value);
		}
#endif
		private ObservableCollection<ulong> _UserList = new();

		public static readonly DirectProperty<NumberBox, string> TextProperty =
			AvaloniaProperty.RegisterDirect<NumberBox, string>(
				nameof(Text),
				o => o.Text,
				(o, v) => o.Text = v,
				defaultBindingMode: BindingMode.TwoWay,
				enableDataValidation: true);

		public string Text
		{
			get => _Text;
			set
			{
				SetAndRaise(TextProperty, ref _Text, value);
				CurrentId = ulong.Parse(value);
			}
		}
		private string _Text;

		public static readonly DirectProperty<UserListEditor, ulong> CurrentIdProperty =
			AvaloniaProperty.RegisterDirect<UserListEditor, ulong>(
				nameof(CurrentId),
				o => o.CurrentId,
				(o, v) => o.CurrentId = v,
				defaultBindingMode: BindingMode.TwoWay,
				enableDataValidation: true);

		public ulong CurrentId
		{
			get => _CurrentId;
			set
			{
				if (Text != value.ToString())
				{
					Text = value.ToString();
				}
				SetAndRaise(CurrentIdProperty, ref _CurrentId, value);
			}
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
			_Text = CurrentId.ToString();

			ModifyListCommand = ReactiveCommand.Create<string>(x =>
			{
				//Capture variable due to UI changes
				var id = CurrentId;
				if (!bool.Parse(x))
				{
					_UserList.Remove(id);
				}
				else if (!_UserList.Contains(id))
				{
					_UserList.Add(id);
				}
				CurrentId = 0;
			}, this.WhenAnyValue(x => x.HasError, x => !x));
			InitializeComponent();

			var input = this.FindControl<TextBox>("Input");
			var errorBinding = DataValidationErrors.HasErrorsProperty.Bind().WithMode(BindingMode.OneWay);
			this[HasErrorProperty.Bind()] = input[errorBinding];
		}

		private void InitializeComponent()
			=> AvaloniaXamlLoader.Load(this);
	}
}