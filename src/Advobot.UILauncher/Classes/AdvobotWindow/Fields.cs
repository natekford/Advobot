using Advobot.UILauncher.Actions;
using ICSharpCode.AvalonEdit;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes.AdvobotWindow
{
	public partial class AdvobotWindow : Window
	{
		#region Output
		private readonly Grid _OutputSearchLayout = new Grid
		{
			//Background = UIModification.MakeBrush("#BF000000"),
			Visibility = Visibility.Collapsed,
		};
		private readonly Grid _OutputSearchTextLayout = new Grid();
		private readonly TextBox _OutputSearchResults = new AdvobotTextBox
		{
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			IsReadOnly = true,
		};
		private readonly ComboBox _OutputSearchComboBox = new AdvobotComboBox
		{
			IsEditable = true,
			FontResizeValue = .022,
		};
		private readonly Button _OutputSearchButton = new AdvobotButton
		{
			Content = "Search",
		};
		private readonly Button _OutputSearchCloseButton = new AdvobotButton
		{
			Content = "Close",
		};
		#endregion

		#region Guild Menu
		private readonly Grid _GuildSearchLayout = new Grid
		{
			//Background = UIModification.MakeBrush("#BF000000"),
			Visibility = Visibility.Collapsed
		};
		private readonly Grid _GuildSearchTextLayout = new Grid();
		private readonly Viewbox _GuildSearchNameHeader = null;// UIModification.MakeStandardViewBox("Guild Name:");
		private readonly TextBox _GuildSearchNameInput = new AdvobotTextBox
		{
			MaxLength = 100,
			FontResizeValue = .060,
		};
		private readonly Viewbox _GuildSearchIDHeader = null;// UIModification.MakeStandardViewBox("ID:");
		private readonly TextBox _GuildSearchIDInput = new AdvobotNumberBox
		{
			MaxLength = 18,
			FontResizeValue = .060,
		};
		private readonly ComboBox _GuildSearchFileComboBox = new AdvobotComboBox();// AdvobotComboBox.CreateEnumComboBox<FileType>(null);
		private readonly Button _GuildSearchSearchButton = new AdvobotButton
		{
			Content = "Search",
		};
		private readonly Button _GuildSearchCloseButton = new AdvobotButton
		{
			Content = "Close",
		};
		#endregion
	}
}
