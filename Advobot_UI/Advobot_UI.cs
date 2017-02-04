using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows.Input;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Media;

namespace Advobot
{
	//Create the UI
	public class BotWindow : Window
	{
		//Layout
		private static Grid mLayout = new Grid()
		{
			Background = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Graphic_Design.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())),
		};

		//Output
		private static TextBox mOutputBox = new TextBox
		{
			//Ints
			Height = 350,
			Width = 750,
			//Bools
			IsReadOnly = true,
			//Enums
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
			TextWrapping = TextWrapping.Wrap,
			//Colors
			//Background = Brushes.Transparent,
			//Foreground = Brushes.White,
		};

		//Input
		private static TextBox mInputBox = new TextBox
		{
			//Ints
			Height = 40,
			Width = 700,
			MaxLength = 100,
			//Bools
			IsReadOnly = false,
			//Enums
			VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
			TextWrapping = TextWrapping.Wrap,
			//Colors
			//Background = Brushes.Transparent,
			//Foreground = Brushes.White,
		};

		//Input button
		private static Button mInputButton = new Button
		{
			//Ints
			Height = 40,
			Width = 50,
			//Bools
			IsEnabled = false,
			//Strings
			Content = "Enter",
		};

		//Secondary grids
		private static Grid mInputGrid = new Grid();

		public BotWindow()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			//Main Grid
			mLayout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(350) });
			mLayout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
			mLayout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(750) });

			//Secondary Grid
			mInputGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
			mInputGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(700) });
			mInputGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(50) });

			//Output Textbox
			mLayout.Children.Add(mOutputBox);
			Grid.SetRow(mLayout, 0);
			Grid.SetColumn(mLayout, 0);

			//Input Grid
			mLayout.Children.Add(mInputGrid);
			Grid.SetRow(mInputGrid, 1);
			Grid.SetColumn(mInputGrid, 0);
			//Input Textbox
			mInputGrid.Children.Add(mInputBox);
			Grid.SetRow(mInputBox, 0);
			Grid.SetColumn(mInputBox, 0);
			//Input Button
			mInputGrid.Children.Add(mInputButton);
			Grid.SetRow(mInputButton, 0);
			Grid.SetColumn(mInputButton, 1);

			//Set this panel as the content for this window.
			Content = mLayout;

			//Do all the actions in this method instead of the initialize one
			runApplication();
		}

		//Do all the actions
		private void runApplication()
		{
			//Make console output show on the output text box
			Console.SetOut(new TextBoxStreamWriter(mOutputBox));

			Task.Run(async () =>
			{
				//Check if valid path at startup
				Variables.GotPath = Actions.validatePathText(Properties.Settings.Default.Path, true);
				//Check if valid key at startup
				Variables.GotKey = Variables.GotPath && await Actions.validateBotKey(Variables.Client, Properties.Settings.Default.BotKey, true);
				//Try to start the bot
				Actions.maybeStartBot();
			});

			//Enable button on typing. Allow commands to be sent through UI
			mInputBox.KeyUp += new KeyEventHandler(acceptInput);
			mInputButton.Click += new RoutedEventHandler(acceptInput);
		}

		//Accept input through the enter key
		private void acceptInput(object sender, KeyEventArgs key)
		{
			if (String.IsNullOrWhiteSpace(mInputBox.Text))
			{
				mInputButton.IsEnabled = false;
				return;
			}
			else
			{
				if (key.Key.Equals(Key.Enter) || key.Key.Equals(Key.Return))
				{
					gatherInput();
				}
				else
				{
					mInputButton.IsEnabled = true;
				}
			}
		}

		//Accept input through the enter button
		private void acceptInput(object sender, RoutedEventArgs mouse)
		{
			gatherInput();
		}

		//Gather the input and reset the input
		private void gatherInput()
		{
			//Get the current text
			var text = mInputBox.Text;
			//Write it out to the ghetto console
			Console.WriteLine(text);
			//Clear the input
			mInputBox.Text = "";
			//Reset the enter button
			mInputButton.IsEnabled = false;
			//Do an action with the text
			Task.Run(async () =>
			{
				//Get the input
				if (!Variables.GotPath)
				{
					Variables.GotPath = Actions.validatePathText(text);
					Variables.GotKey = Variables.GotPath && await Actions.validateBotKey(Variables.Client, Properties.Settings.Default.BotKey, true);
				}
				else if (!Variables.GotKey)
				{
					Variables.GotKey = await Actions.validateBotKey(Variables.Client, text);
				}
				Actions.maybeStartBot();
			});
		}
	}

	//Write the console output into the UI
	public class TextBoxStreamWriter : TextWriter
	{
		TextBox mOutput = null;

		public TextBoxStreamWriter(TextBox output)
		{
			mOutput = output;
		}

		public override void Write(char value)
		{
			base.Write(value);
			mOutput.Dispatcher.Invoke(() =>
			{
				mOutput.AppendText(value.ToString());
			});
		}

		public override Encoding Encoding
		{
			get { return Encoding.UTF8; }
		}
	}
}
