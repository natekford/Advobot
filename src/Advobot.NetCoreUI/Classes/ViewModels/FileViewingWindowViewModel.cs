using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Advobot.NetCoreUI.Classes.Views;
using Advobot.NetCoreUI.Utils;
using AdvorangesUtils;
using Avalonia.Controls;
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
		private string _WindowTitle;

		public string Output
		{
			get => _Output;
			set
			{
				this.RaiseAndSetIfChanged(ref _Output, value);
				_IsDirty = value.GetHashCode() != _LastSaved;
			}
		}
		private string _Output;

		private int _LastSaved;
		private bool _IsDirty;

		public ICommand SaveCommand { get; }
		public ICommand CopyCommand { get; }
		public ICommand CloseCommand { get; }
		public ICommand DeleteCommand { get; }

		private readonly FileInfo _File;
		private readonly Type _FileType;

		public FileViewingWindowViewModel(FileInfo file, Type fileType = null)
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
			DeleteCommand = ReactiveCommand.CreateFromTask(Delete);
		}

		private void Save(FileInfo file, string value, string caller)
		{
			DoIO(() =>
			{
				var response = file.Save(value, _FileType);
				//Only update the last saved info if it was actually saved
				if (response == SaveResponse.Success)
				{
					_LastSaved = value.GetHashCode();
					_IsDirty = false;
				}
				ConsoleUtils.WriteLine(response.GetSaveResponse(_File), name: caller);
			});
		}
		private void DoIO(Action callback)
		{
			try
			{
				callback();
			}
			catch (Exception e)
			{
				e.Write();
			}
		}
		private void Save()
			=> Save(_File, Output, "Saving File");
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
				Save(new FileInfo(newPath), Output, "Copying File");
			}
		}
		private async Task Close(Window window)
		{
			var msg = $"There are unsaved changes. Are you sure you want to close the file {_File.Name}?";
			if (!_IsDirty || await MessageBox.Show(msg, _Caption, new[] { "Yes", "No" }) == "Yes")
			{
				window?.Close();
			}
		}
		private async Task Delete()
		{
			var msg = $"Are you sure you want to delete the file {_File.Name}?";
			if (await MessageBox.Show(msg, _Caption, new[] { "Yes", "No" }) == "Yes")
			{
				DoIO(() =>
				{
					_File.Delete();
					ConsoleUtils.WriteLine($"Successfully deleted the file {_File}.");
				});
			}
		}
	}
}
