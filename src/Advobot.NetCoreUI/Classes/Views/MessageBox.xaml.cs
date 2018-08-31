using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.NetCoreUI.Classes.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Advobot.NetCoreUI.Classes.Views
{
	public sealed class MessageBox : Window
	{
		private MessageBox()
		{
			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public static Task<string> Show(string messageBoxText, string caption, IEnumerable<string> options)
		{
			return new MessageBox
			{
				DataContext = new MessageBoxViewModel
				{
					Text = messageBoxText,
					WindowTitle = caption,
					Options = options,
				},
			}.ShowDialog<string>();
		}
	}
}
