using Advobot.UILauncher.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Advobot.UILauncher
{
    /// <summary>
    /// Interaction logic for OutputSearchWindow.xaml
    /// </summary>
    public partial class OutputSearchWindow : Window
    {
		public OutputSearchWindow(Window mainWindow) : this()
		{
			this.Owner = mainWindow;
			this.Resources = mainWindow.Resources;
			this.Height = mainWindow.Height / 2;
			this.Width = mainWindow.Width / 2;
		}
		public OutputSearchWindow()
        {
            InitializeComponent();
			ColorSettings.SwitchElementColorOfChildren(this.Layout);
		}

		private void WindowClosed(object sender, EventArgs e)
		{
			//Restore opacity and return false, indicating the user did not search
			this.Owner.Opacity = 100;
			if (this.DialogResult == null)
			{
				this.DialogResult = false;
			}
		}
		private void Close(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		private void Search(object sender, RoutedEventArgs e)
		{

		}
		private void Save(object sender, RoutedEventArgs e)
		{

		}
	}
}
