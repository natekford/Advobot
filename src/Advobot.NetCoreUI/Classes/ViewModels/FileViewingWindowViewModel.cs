using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Advobot.NetCoreUI.Classes.Views;
using Advobot.NetCoreUI.Utils;
using AdvorangesUtils;
using Avalonia.Controls;
using Avalonia.Media;
using ReactiveUI;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public sealed class FileViewingWindowViewModel : ReactiveObject
	{
		private static readonly string _Caption = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;

		public string WindowTitle
		{
			get => _WindowTitle;
			set => this.RaiseAndSetIfChanged(ref _WindowTitle, value);
		}
		private string _WindowTitle = "";

		public string SavingText
		{
			get => _SavingText;
			set => this.RaiseAndSetIfChanged(ref _SavingText, value);
		}
		private string _SavingText = "";

		public ISolidColorBrush SavingBackground
		{
			get => _SavingBackground;
			set => this.RaiseAndSetIfChanged(ref _SavingBackground, value);
		}
		private ISolidColorBrush _SavingBackground = Brushes.Yellow;

		public bool SavingOpen
		{
			get => _SavingOpen;
			set => this.RaiseAndSetIfChanged(ref _SavingOpen, value);
		}
		private bool _SavingOpen;

		public string Output
		{
			get => _Output;
			set
			{
				this.RaiseAndSetIfChanged(ref _Output, value);
				_IsDirty = value.GetHashCode() != _LastSaved;
			}
		}
		private string _Output = "";

		private int _LastSaved;
		private bool _IsDirty;

		public ICommand SaveCommand { get; }
		public ICommand CopyCommand { get; }
		public ICommand CloseCommand { get; }
		public ICommand DeleteCommand { get; }

		private readonly FileInfo _File;
		private readonly Type? _FileType;
		private CancellationTokenSource? _SavingNotificationCancelToken;

		public FileViewingWindowViewModel(FileInfo file, Type? fileType = null)
		{
			_File = file;
			_FileType = fileType;

			WindowTitle = $"Advobot - Currently viewing {_File}";
			Output = File.ReadAllText(file.FullName);
			_LastSaved = Output.GetHashCode();
			_IsDirty = false;

			SaveCommand = ReactiveCommand.Create(Save);
			CopyCommand = ReactiveCommand.CreateFromTask<Window>(Copy);
			CloseCommand = ReactiveCommand.CreateFromTask<Window>(Close);
			DeleteCommand = ReactiveCommand.CreateFromTask<Window>(Delete);
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
				await Task.Delay(5000, token);
				SavingOpen = false;
			});

			ConsoleUtils.WriteLine(text, name: caller);
		}
		private void Save()
			=> Save(_File, Output);
		private async Task Copy(Window window)
		{
			var newPath = await new SaveFileDialog
			{
				InitialDirectory = _File.Directory.FullName,
				InitialFileName = _File.FullName,
				Title = "Advobot - File Copying",
			}.ShowAsync(window);
			if (newPath != null)
			{
				Save(new FileInfo(newPath), Output);
			}
		}
		private async Task Close(Window window)
		{
			var msg = $"There are unsaved changes. Are you sure you want to close the file {_File.Name}?";
			if (!_IsDirty || await MessageBox.ShowAsync(window, msg, _Caption, new[] { "Yes", "No" }) == "Yes")
			{
				window?.Close();
			}
		}
		private async Task Delete(Window window)
		{
			var msg = $"Are you sure you want to delete the file {_File.Name}?";
			if (await MessageBox.ShowAsync(window, msg, _Caption, new[] { "Yes", "No" }) == "Yes")
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
	}
}
