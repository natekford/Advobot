using Advobot.Core.Actions;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Classes;
using Advobot.UILauncher.Classes.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Advobot.UILauncher.Windows
{
    /// <summary>
    /// Interaction logic for OutputSearchWindow.xaml
    /// </summary>
    public partial class OutputSearchWindow : ModalWindow
	{
		public OutputSearchWindow(Window mainWindow) : base(mainWindow)
		{
			InitializeComponent();
			ColorSettings.SwitchElementColorOfChildren(this.Layout);
			this.OutputNamesComboBox.ItemsSource = AdvobotComboBox.CreateComboBoxSourceOutOfStrings(ConsoleActions.GetWrittenLines().Keys.ToArray());
		}
		public OutputSearchWindow() : this(null) { }

		private void Search(object sender, RoutedEventArgs e)
		{
			if (this.OutputNamesComboBox.SelectedItem is TextBox tb)
			{
				this.ConsoleSearchOutput.Clear();
				foreach (var line in ConsoleActions.GetWrittenLines()[tb.Text])
				{
					this.ConsoleSearchOutput.AppendText($"{line}{Environment.NewLine}");
				}
			}
		}
		private void SaveWithCtrlS(object sender, KeyEventArgs e)
		{
			if (SavingActions.IsCtrlS(e))
			{
				Save(sender, e);
			}
		}
		private async void Save(object sender, RoutedEventArgs e)
		{
			//TODO: Get this tooltip to work
			//TODO: also figure out the file search crash bug with drop down selected and some text in left box, and hit red x
			var response = SavingActions.SaveFile(this.ConsoleSearchOutput);
			await ToolTipActions.EnableTimedToolTip(this.Owner, response.GetReason());
		}
	}
}
