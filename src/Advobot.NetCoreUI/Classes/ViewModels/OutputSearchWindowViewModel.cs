using ReactiveUI;
using Advobot.Interfaces;
using System.Collections.Generic;
using AdvorangesUtils;
using System.Windows.Input;
using System;
using Advobot.NetCoreUI.Utils;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
    public class OutputSearchWindowViewModel : ReactiveObject
    {
        public string SearchTerm
        {
            get => _SearchTerm;
            set => this.RaiseAndSetIfChanged(ref _SearchTerm, value);
        }
        private string _SearchTerm;

        public string Output
        {
            get => _Output;
            set => this.RaiseAndSetIfChanged(ref _Output, value);
        }
        private string _Output;
		public string SaveResponse
		{
			get => _SaveResponse;
			set => this.RaiseAndSetIfChanged(ref _SaveResponse, value);
		}
		private string _SaveResponse;

        public IEnumerable<string> Keys => ConsoleUtils.WrittenLines.Keys;

        public ICommand SearchCommand { get; }
        public ICommand SaveCommand { get; }

        private readonly IBotDirectoryAccessor _Accessor;

        public OutputSearchWindowViewModel() : this(null) { }
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
            });
            SaveCommand = ReactiveCommand.Create(() =>
            {
            	if (Output.Length == 0)
			    {
					return;
			    }
				var success = NetCoreUIUtils.Save(_Accessor, "Output_Search", Output);
				SaveResponse = NetCoreUIUtils.GetSaveResponse(success);
    		});
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