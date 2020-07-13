using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Advobot.UI.Utils;
using Advobot.UI.Views;

using AdvorangesUtils;

using Avalonia.Controls;
using Avalonia.Media;

using ReactiveUI;

namespace Advobot.UI.ViewModels
{
	public sealed class FileViewingWindowViewModel : ReactiveObject
	{
		private static readonly string _Caption = Assembly.GetExecutingAssembly()
			.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "";

		private readonly FileInfo _File;
		private readonly Type? _FileType;
		private bool _IsDirty;
		private int _LastSaved;
		private string _Output;
		private ISolidColorBrush _SavingBackground = Brushes.Yellow;
		private CancellationTokenSource? _SavingNotificationCancelToken;
		private bool _SavingOpen;
		private string _SavingText = "";
		private string _WindowTitle = "";
		public ICommand CloseCommand { get; }
		public ICommand CopyCommand { get; }
		public ICommand DeleteCommand { get; }

		public string Output
		{
			get => _Output;
			set
			{
				this.RaiseAndSetIfChanged(ref _Output, value);
				_IsDirty = value.GetHashCode() != _LastSaved;
			}
		}

		public ICommand SaveCommand { get; }

		public ISolidColorBrush SavingBackground
		{
			get => _SavingBackground;
			set => this.RaiseAndSetIfChanged(ref _SavingBackground, value);
		}

		public bool SavingOpen
		{
			get => _SavingOpen;
			set => this.RaiseAndSetIfChanged(ref _SavingOpen, value);
		}

		public string SavingText
		{
			get => _SavingText;
			set => this.RaiseAndSetIfChanged(ref _SavingText, value);
		}

		public string WindowTitle
		{
			get => _WindowTitle;
			set => this.RaiseAndSetIfChanged(ref _WindowTitle, value);
		}

		public FileViewingWindowViewModel(FileInfo file, Type? fileType = null)
		{
			_File = file;
			_FileType = fileType;

			WindowTitle = $"Advobot - Currently viewing {_File}";
			_Output = File.ReadAllText(file.FullName);
			_LastSaved = Output.GetHashCode();
			_IsDirty = false;

			SaveCommand = ReactiveCommand.Create(Save);
			CopyCommand = ReactiveCommand.CreateFromTask<Window>(Copy);
			CloseCommand = ReactiveCommand.CreateFromTask<Window>(Close);
			DeleteCommand = ReactiveCommand.CreateFromTask<Window>(Delete);
		}

		private async Task Close(Window window)
		{
			const string YES = "Yes";
			const string NO = "No";
			var msg = $"There are unsaved changes. Are you sure you want to close the file {_File.Name}?";

			var option = await MessageBox.ShowAsync(window, msg, _Caption, new[] { YES, NO }).CAF();
			if (!_IsDirty || option == YES)
			{
				window?.Close();
			}
		}

		private async Task Copy(Window window)
		{
			if (_File.Directory == null)
			{
				return;
			}

			var newPath = await new SaveFileDialog
			{
				Directory = _File.Directory.FullName,
				InitialFileName = _File.FullName,
				Title = "Advobot - File Copying",
			}.ShowAsync(window).ConfigureAwait(true);
			if (newPath != null)
			{
				Save(new FileInfo(newPath), Output);
			}
		}

		private async Task Delete(Window window)
		{
			const string YES = "Yes";
			const string NO = "No";

			var msg = $"Are you sure you want to delete the file {_File.Name}?";
			var option = await MessageBox.ShowAsync(window, msg, _Caption, new[] { YES, NO }).CAF();
			if (option == YES)
			{
				try
				{
					_File.Delete();
					HandleResponse($"Successfully deleted the file {_File}.", Brushes.Yellow);
				}
				catch (Exception e)
				{
					e.Write();
				}
			}
		}

		private void HandleResponse(string text, ISolidColorBrush brush, [CallerMemberName] string caller = "")
		{
			_SavingNotificationCancelToken?.Cancel();
			_SavingNotificationCancelToken?.Dispose();
			var token = (_SavingNotificationCancelToken = new CancellationTokenSource()).Token;

			SavingText = text;
			SavingBackground = brush;
			SavingOpen = true;

			//Run this on a background thread since it isn't intended to block
			Task.Run(async () =>
			{
				await Task.Delay(5000, token).CAF();
				SavingOpen = false;
			});

			ConsoleUtils.WriteLine(text, name: caller);
		}

		private void Save(FileInfo file, string value, [CallerMemberName] string caller = "")
		{
			var response = file.Save(value, _FileType);
			//Only update the last saved info if it was actually saved
			if (response == SaveStatus.Success)
			{
				_LastSaved = value.GetHashCode();
				_IsDirty = false;
			}

			var (text, brush) = response.GetSaveResponse(_File);
			HandleResponse(text, brush, caller);
		}

		private void Save()
			=> Save(_File, Output);
	}
}