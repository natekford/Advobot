using System;
using System.Collections.Generic;
using System.Windows.Input;
using Advobot.Interfaces;
using Advobot.NetCoreUI.Utils;
using AdvorangesUtils;
using Avalonia.Controls;
using ReactiveUI;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public class OutputSearchWindowViewModel : ReactiveObject
	{
		public string SearchTerm
		{
			get => _SearchTerm;
			set
			{
				this.RaiseAndSetIfChanged(ref _SearchTerm, value);
				CanSearch = !string.IsNullOrWhiteSpace(value);
			}
		}
		private string _SearchTerm;

		private bool CanSearch
		{
			get => _CanSearch;
			set => this.RaiseAndSetIfChanged(ref _CanSearch, value);
		}
		private bool _CanSearch;

		public string Output
		{
			get => _Output;
			set
			{
				this.RaiseAndSetIfChanged(ref _Output, value);
				CanSaveOutput = value.Length > 0;
			}
		}
		private string _Output;

		private bool CanSaveOutput
		{
			get => _CanSaveOutput;
			set => this.RaiseAndSetIfChanged(ref _CanSaveOutput, value);
		}
		private bool _CanSaveOutput;

		public string SaveResponse
		{
			get => _SaveResponse;
			set => this.RaiseAndSetIfChanged(ref _SaveResponse, value);
		}
		private string _SaveResponse;

		public IEnumerable<string> Keys => ConsoleUtils.WrittenLines.Keys;

		public ICommand SearchCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand CloseCommand { get; }

		private readonly IBotDirectoryAccessor _Accessor;

		public OutputSearchWindowViewModel(IBotDirectoryAccessor accessor)
		{
			_Accessor = accessor;

			SearchCommand = ReactiveCommand.Create(() =>
			{
				Output = "";
				foreach (var line in ConsoleUtils.WrittenLines[SearchTerm])
				{
					Output += $"{line}{Environment.NewLine}";
				}
			}, this.WhenAnyValue(x => x.CanSearch));
			SaveCommand = ReactiveCommand.Create(() =>
			{
				if (Output.Length == 0)
				{
					return;
				}
				var success = NetCoreUIUtils.Save(_Accessor, "Output_Search", Output);
				//TODO: make this notify in the window instead of only in main window
				ConsoleUtils.WriteLine(NetCoreUIUtils.GetSaveResponse(success), name: "Saving Output Search");
			}, this.WhenAnyValue(x => x.CanSaveOutput));
			CloseCommand = ReactiveCommand.Create<Window>(window => window?.Close());
		}
		/*
		private void MoveToolTip(object sender, MouseEventArgs e)
		{
			var fe = (FrameworkElement)sender;
			var tt = (ToolTip)fe.ToolTip;
			var pos = e.GetPosition(fe);
			tt.HorizontalOffset = pos.X + 10;
			tt.VerticalOffset = pos.Y + 10;
        }*/
	}
}