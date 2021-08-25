
using Advobot.UI.ViewModels;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Advobot.UI.Views
{
	public sealed class MessageBox : Window
	{
		public MessageBox()
		{
			InitializeComponent();
		}

		public static Task<string> ShowAsync(Window window, string messageBoxText, string caption, IEnumerable<string> options)
		{
			return new MessageBox
			{
				DataContext = new MessageBoxViewModel
				{
					Text = messageBoxText,
					WindowTitle = caption,
					Options = options,
				},
			}.ShowDialog<string>(window);
		}

		private void InitializeComponent()
			=> AvaloniaXamlLoader.Load(this);
	}
}