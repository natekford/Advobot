using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

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

		#region Output
		//Save ouput
		private static MenuItem mOutputContextMenuSave = new MenuItem
		{
			Header = "Save Output Log",
		};
		//Clear output
		private static MenuItem mOutputContextMenuClear = new MenuItem
		{
			Header = "Clear Output Log",
		};
		//Create the context menu for the output window
		private static ContextMenu mOutputContextMenu = new ContextMenu
		{
			ItemsSource = new[] { mOutputContextMenuSave, mOutputContextMenuClear },
		};
		//Output textbox
		private static RichTextBox mOutput = new RichTextBox
		{
			ContextMenu = mOutputContextMenu,
			IsReadOnly = true,
			IsDocumentEnabled = true,
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			Background = Brushes.White,
		};
		//Secondary output textbox
		private static RichTextBox mSecondaryOutput = new RichTextBox
		{
			Background = Brushes.White,
			IsReadOnly = true,
			IsDocumentEnabled = true,
			Visibility = Visibility.Hidden,
		};
		//Secondary output button names
		private const string mFirstButtonString = "Help";
		private const string mSecondButtonString = "Commands";
		private const string mThirdButtonString = "Info";
		private const string mFourthButtonString = "Settings";
		private static string mLastButtonClicked;
		//Inlines
		private static Run mFirstHelpPart = new Run("Command Syntax:\n\t[] means required\n\t<> means optional\n\t| means or\n\nLatency:\n\tTime it takes for a command to reach the bot.\nMemory:\n\t" +
			"Amount of RAM the program is using.\nThreads:\n\tWhere all the actions in the bot happen.\nShards:\n\tHold all the guilds a bot has on its client.\n\tThere is a limit of 2500 guilds per shard." +
			"\n\nCurrent API Wrapper Version: " + Constants.API_VERSION + "\nCurrent Bot Version: " + Constants.BOT_VERSION);
		private static Run mSecondHelpPart = new Run("\nCurrent GitHub Repository: ");
		private static Hyperlink mThirdHelpPart = CreateHyperlink("https://github.com/advorange/Advobot", "Advobot");
		private static Run mFourthHelpPart = new Run("\n\nCharacter Count: 350,000+\nLine Count: 10,000+");
		//Paragraphs
		private static Paragraph mFirstParagraph = new Paragraph();
		private static Paragraph mSecondParagraph = new Paragraph(new Run("Placeholder"));
		private static Paragraph mThirdParagraph = new Paragraph(new Run("Lorem Ipsum"));
		private static Paragraph mFourthParagraph = new Paragraph(new Run("Test"));
		//Button layout
		private static Grid mButtonLayout = new Grid();
		//First button
		private static Button mFirstButton = new Button
		{
			Content = mFirstButtonString,
		};
		//Second button
		private static Button mSecondButton = new Button
		{
			Content = mSecondButtonString,
		};
		//Third button
		private static Button mThirdButton = new Button
		{
			Content = mThirdButtonString,
		};
		//Fourth button
		private static Button mFourthButton = new Button
		{
			Content = mFourthButtonString,
		};
		#endregion

		#region Input
		//Input layout
		private static Grid mInputLayout = new Grid();
		//Input textbox
		private static RichTextBox mInput = new RichTextBox
		{
			Background = Brushes.White,
			Margin = new Thickness(0),
		};
		//Input button
		private static Button mInputButton = new Button
		{
			IsEnabled = false,
			Content = "Enter",
		};
		#endregion

		#region System Info
		//System info layout
		private static Grid mSysInfoLayout = new Grid();
		//System info underneath
		private static TextBox mSysInfoUnder = new TextBox();
		//Latency
		private static TextBox mLatency = new TextBox
		{
			IsReadOnly = true,
			BorderBrush = Brushes.Transparent,
		};
		//Latency viewbox
		private static Viewbox mLatencyView = new Viewbox
		{
			Child = mLatency,
		};
		//Memory
		private static TextBox mMemory = new TextBox
		{
			IsReadOnly = true,
			BorderBrush = Brushes.Transparent,
		};
		//Memory viewbox
		private static Viewbox mMemoryView = new Viewbox
		{
			Child = mMemory,
		};
		//Memory tooltip
		private static ToolTip mMemHoverInfo = new ToolTip
		{
			Content = "This is not guaranteed to be 100% correct.",
		};
		//Threads
		private static TextBox mThreads = new TextBox
		{
			IsReadOnly = true,
			BorderBrush = Brushes.Transparent,
		};
		//Threads viewbox
		private static Viewbox mThreadsView = new Viewbox
		{
			Child = mThreads,
		};
		//Shards
		private static TextBox mShards = new TextBox
		{
			IsReadOnly = true,
			BorderBrush = Brushes.Transparent,
		};
		//Shards viewbox
		private static Viewbox mShardsView = new Viewbox
		{
			Child = mShards,
		};
		//Prefix
		private static TextBox mPrefix = new TextBox
		{
			IsReadOnly = true,
			BorderBrush = Brushes.Transparent,
		};
		//Prefix viewbox
		private static Viewbox mPrefixView = new Viewbox
		{
			Child = mPrefix,
			HorizontalAlignment = HorizontalAlignment.Stretch,
		};
		#endregion

		#region Bindings
		//Text resize binding
		private static Binding mInputBinding = new Binding
		{
			Path = new PropertyPath("ActualHeight"),
			RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1),
			Converter = new FontResizer(.275),
		};
		private static Binding mSecondaryOutputBiding = new Binding
		{
			Path = new PropertyPath("ActualHeight"),
			RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1),
			Converter = new FontResizer(.0175),
		};
		#endregion

		#region Creating the Window
		//Create the bot window
		public BotWindow()
		{
			InitializeComponent();
		}

		//Create all the components
		private void InitializeComponent()
		{
			//Main layout
			AddRows(mLayout, 100);
			AddCols(mLayout, 4);

			//Output
			AddItemAndSetPositionsAndSpans(mLayout, mOutput, 0, 90, 0, 4);
			mSecondaryOutput.SetBinding(FontSizeProperty, mSecondaryOutputBiding);
			AddItemAndSetPositionsAndSpans(mLayout, mSecondaryOutput, 0, 90, 3, 1);

			//System Info
			AddItemAndSetPositionsAndSpans(mLayout, mSysInfoLayout, 90, 3, 0, 3, 0, 5);
			AddItemAndSetPositionsAndSpans(mSysInfoLayout, mSysInfoUnder, 0, 1, 0, 5);
			AddItemAndSetPositionsAndSpans(mSysInfoLayout, mLatencyView, 0, 1, 0, 1);
			AddItemAndSetPositionsAndSpans(mSysInfoLayout, mMemoryView, 0, 1, 1, 1);
			AddItemAndSetPositionsAndSpans(mSysInfoLayout, mThreadsView, 0, 1, 2, 1);
			AddItemAndSetPositionsAndSpans(mSysInfoLayout, mShardsView, 0, 1, 3, 1);
			AddItemAndSetPositionsAndSpans(mSysInfoLayout, mPrefixView, 0, 1, 4, 1);

			//Input
			AddItemAndSetPositionsAndSpans(mLayout, mInputLayout, 93, 7, 0, 3, 1, 10);
			mInput.SetBinding(FontSizeProperty, mInputBinding);
			mInput.Document.LineHeight = 1;
			AddItemAndSetPositionsAndSpans(mInputLayout, mInput, 0, 1, 0, 9);
			AddItemAndSetPositionsAndSpans(mInputLayout, mInputButton, 0, 1, 9, 1);

			//Buttons
			AddItemAndSetPositionsAndSpans(mLayout, mButtonLayout, 90, 10, 3, 1, 1, 4);
			AddItemAndSetPositionsAndSpans(mButtonLayout, mFirstButton, 0, 1, 0, 1);
			AddItemAndSetPositionsAndSpans(mButtonLayout, mSecondButton, 0, 1, 1, 1);
			AddItemAndSetPositionsAndSpans(mButtonLayout, mThirdButton, 0, 1, 2, 1);
			AddItemAndSetPositionsAndSpans(mButtonLayout, mFourthButton, 0, 1, 3, 1);

			//Paragraphs
			mFirstParagraph.Inlines.AddRange(new Inline[] { mFirstHelpPart, mSecondHelpPart, mThirdHelpPart, mFourthHelpPart });

			//Set this panel as the content for this window.
			Content = mLayout;
			//Actually run the application
			RunApplication();
		}

		//Actually make the application work
		private void RunApplication()
		{
			//Make console output show on the output text block and box
			Console.SetOut(new TextBoxStreamWriter(mOutput));

			//Validate path/botkey after the UI has launched to have them logged
			Task.Run(async () =>
			{
				//Check if valid path at startup
				Variables.GotPath = Actions.ValidatePath(Properties.Settings.Default.Path, true);
				//Check if valid key at startup
				Variables.GotKey = Variables.GotPath && await Actions.ValidateBotKey(Variables.Client, Properties.Settings.Default.BotKey, true);
				//Try to start the bot
				Actions.MaybeStartBot();
			});

			//Make sure the system information stays updated
			UpdateSystemInformation();

			//Events
			mInput.KeyUp += AcceptInput;
			mInputButton.Click += AcceptInput;
			mOutputContextMenuSave.Click += SaveOutput;
			mOutputContextMenuClear.Click += ClearOutput;
			mFirstButton.Click += BringUpMenu;
			mSecondButton.Click += BringUpMenu;
			mThirdButton.Click += BringUpMenu;
			mFourthButton.Click += BringUpMenu;
			mMemory.MouseEnter += ModifyMemHoverInfo;
			mMemory.MouseLeave += ModifyMemHoverInfo;
		}

		//Add in X amount of rows
		private static void AddRows(Grid grid, int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				grid.RowDefinitions.Add(new RowDefinition());
			}
		}

		//Add in X amount of columns
		private static void AddCols(Grid grid, int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				grid.ColumnDefinitions.Add(new ColumnDefinition());
			}
		}

		//Set the row and row span
		private static void SetRowAndSpan(UIElement item, int start = 0, int length = 1)
		{
			Grid.SetRow(item, start < 0 ? 0 : start);
			Grid.SetRowSpan(item, length <= 0 ? 1 : length);
		}

		//Set the colum and column span
		private static void SetColAndSpan(UIElement item, int start = 0, int length = 1)
		{
			Grid.SetColumn(item, start < 0 ? 0 : start);
			Grid.SetColumnSpan(item, length <= 0 ? 1 : length);
		}

		//Add the child to the given grid with specified starting rows/columns and lengths
		private static void AddItemAndSetPositionsAndSpans(Panel parent, UIElement child, int rowStart, int rowLength, int columnStart, int columnLength, int setRows = 0, int setColumns = 0)
		{
			if (child is Grid)
			{
				AddRows(child as Grid, setRows);
				AddCols(child as Grid, setColumns);
			}
			parent.Children.Add(child);
			SetRowAndSpan(child, rowStart, rowLength);
			SetColAndSpan(child, columnStart, columnLength);
		}
		#endregion

		#region Events
		//Accept input through the enter key
		private void AcceptInput(object sender, KeyEventArgs e)
		{
			var text = new TextRange(mInput.Document.ContentStart, mInput.Document.ContentEnd).Text;
			if (String.IsNullOrWhiteSpace(text))
			{
				mInputButton.IsEnabled = false;
				return;
			}
			else
			{
				if (e.Key.Equals(Key.Enter) || e.Key.Equals(Key.Return))
				{
					GatherInput();
				}
				else
				{
					if (text.Length > 250)
					{

					}
					mInputButton.IsEnabled = true;
				}
			}
		}

		//Accept input through the enter button
		private void AcceptInput(object sender, RoutedEventArgs e)
		{
			GatherInput();
		}

		//Save the output after the button is clicked
		private void SaveOutput(object sender, RoutedEventArgs e)
		{
			//Make sure the path is valid
			var path = Actions.GetServerFilePath(0, "Output_Log_" + DateTime.UtcNow.ToString("MM-dd_HH-mm-ss") + ".txt", true);
			if (path == null)
			{
				Actions.WriteLine("Unable to save the output log.");
				return;
			}

			//Create the temporary file
			if (!File.Exists(path))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
			}

			//Save the file
			using (FileStream stream = new FileStream(path, FileMode.Create))
			{
				new TextRange(mOutput.Document.ContentStart, mOutput.Document.ContentEnd).Save(stream, DataFormats.Text, true);
			}

			//Write to the console telling the user that the console log was successfully saved
			Actions.WriteLine("Successfully saved the output log.");
		}

		//Clear the output after the button is clicked
		private void ClearOutput(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show("Are you sure you want to clear the output window?", Variables.Bot_Name, MessageBoxButton.OKCancel);

			switch (result)
			{
				case MessageBoxResult.OK:
				{
					mOutput.Document.Blocks.Clear();
					break;
				}
				case MessageBoxResult.Cancel:
				{
					break;
				}
			}
		}

		//Bring up the associated menu
		private void BringUpMenu(object sender, RoutedEventArgs e)
		{
			//Get the button's name
			var name = ((Button)sender).Content.ToString();
			//Remove the current blocks in the document
			mSecondaryOutput.Document.Blocks.Clear();
			//Disable the rtb if the most recent button clicked is clicked again
			if (name.Equals(mLastButtonClicked, StringComparison.OrdinalIgnoreCase))
			{
				SetColAndSpan(mOutput, 0, 4);
				mSecondaryOutput.Visibility = Visibility.Hidden;
				mLastButtonClicked = null;
			}
			else
			{
				//Resize the regular output window
				SetColAndSpan(mOutput, 0, 3);
				//Make the secondary output visible
				mSecondaryOutput.Visibility = Visibility.Visible;
				//Keep track of the last button clicked
				mLastButtonClicked = name;

				//Show the text for help
				if (name.Equals(mFirstButtonString, StringComparison.OrdinalIgnoreCase))
				{
					mSecondaryOutput.Document.Blocks.Add(mFirstParagraph);
				}
				//Show the text for commands
				else if (name.Equals(mSecondButtonString, StringComparison.OrdinalIgnoreCase))
				{
					mSecondaryOutput.Document.Blocks.Add(mSecondParagraph);
				}
				//Show the text for info
				else if (name.Equals(mThirdButtonString, StringComparison.OrdinalIgnoreCase))
				{
					mSecondaryOutput.Document.Blocks.Add(mThirdParagraph);
				}
				//Show the text for settings
				else if (name.Equals(mFourthButtonString, StringComparison.OrdinalIgnoreCase))
				{
					mSecondaryOutput.Document.Blocks.Add(mFourthParagraph);
				}
			}
		}

		//Modify the memory tooltip
		private void ModifyMemHoverInfo(object sender, MouseEventArgs e)
		{
			mMemHoverInfo.IsOpen = !mMemHoverInfo.IsOpen;
		}
		#endregion

		//Add a hypderlink to an output box
		public static void AddHyperlink(RichTextBox output, string link, string name, string beforeText = null, string afterText = null)
		{
			//Create the hyperlink
			var hyperlink = CreateHyperlink(link, name);
			if (hyperlink == null)
			{
				return;
			}
			//Check if the paragraph is valid
			var para = (Paragraph)mOutput.Document.Blocks.LastBlock;
			if (para == null)
			{
				Actions.WriteLine(link);
				return;
			}
			//Format the text before the hyperlink
			if (String.IsNullOrWhiteSpace(beforeText))
			{
				para.Inlines.Add(new Run(DateTime.Now.ToString("HH:mm:ss") + ": "));
			}
			else
			{
				para.Inlines.Add(new Run(beforeText));
			}
			//Add in the hyperlink
			para.Inlines.Add(hyperlink);
			//Format the text after the hyperlink
			if (String.IsNullOrWhiteSpace(beforeText))
			{
				para.Inlines.Add(new Run("\r"));
			}
			else
			{
				para.Inlines.Add(new Run(afterText));
			}
			//Add the paragraph to the ouput
			output.Document.Blocks.Add(para);
		}

		//Validate a hyperlink
		public static Hyperlink CreateHyperlink(string link, string name)
		{
			//Make sure the input is a valid link
			if (!Actions.ValidateURL(link))
			{
				Actions.WriteLine(Actions.ERROR("Invalid URL."));
				return null;
			}
			//Create the hyperlink
			var hyperlink = new Hyperlink(new Run(name))
			{
				NavigateUri = new Uri(link),
				IsEnabled = true,
			};
			//Make it work when clicked
			hyperlink.RequestNavigate += (sender, e) =>
			{
				Process.Start(e.Uri.ToString());
				e.Handled = true;
			};
			return hyperlink;
		}

		//Gather the input and reset the input
		private void GatherInput()
		{
			//Get the current text
			var text = new TextRange(mInput.Document.ContentStart, mInput.Document.ContentEnd).Text.Trim(new char[] { '\r', '\n' });
			//Write it out to the ghetto console
			Console.WriteLine(text);
			//Clear the input
			mInput.Document.Blocks.Clear();
			//Reset the enter button
			mInputButton.IsEnabled = false;
			//Do an action with the text
			if (!Variables.GotPath || !Variables.GotKey)
			{
				Task.Run(async () =>
				{
					//Get the input
					if (!Variables.GotPath)
					{
						Variables.GotPath = Actions.ValidatePath(text);
						Variables.GotKey = Variables.GotPath && await Actions.ValidateBotKey(Variables.Client, Properties.Settings.Default.BotKey, true);
					}
					else if (!Variables.GotKey)
					{
						Variables.GotKey = await Actions.ValidateBotKey(Variables.Client, text);
					}
					Actions.MaybeStartBot();
				});
			}
			else
			{
				UICommandHandler.HandleCommand(text);
			}
		}

		//Update the latency/memory/threads every second
		private void UpdateSystemInformation()
		{
			//Create the timer
			var timer = new DispatcherTimer();
			//Make the timer's action
			timer.Tick += (sender, e) =>
			{
				var client = Variables.Client;
				mLatency.Text = String.Format("Latency: {0}ms", client.GetLatency());
				mMemory.Text = String.Format("Memory: {0}MB", (Process.GetCurrentProcess().WorkingSet64 / 1000000.0).ToString("0.00"));
				mThreads.Text = String.Format("Threads: {0}", Process.GetCurrentProcess().Threads.Count);
				mShards.Text = String.Format("Shards: {0}", client.GetShards().Count);
				mPrefix.Text = String.Format("Prefix: {0}", Properties.Settings.Default.Prefix);
			};
			//Make the timer update every so often
			timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
			//Start the timer
			timer.Start();
		}

		//Get the main output box
		public static RichTextBox MainOutput
		{
			get { return mOutput; }
		}

		//Get the secondary output box
		public static RichTextBox SecondaryOutput
		{
			get { return mSecondaryOutput; }
		}
	}

	//New class to handle commands
	public class UICommandHandler
	{
		//Pull out the cmd name and the args
		public static void HandleCommand(string input)
		{
			//Check if it's a global bot command done through the console
			if (input.StartsWith(Properties.Settings.Default.Prefix, StringComparison.OrdinalIgnoreCase))
			{
				//Remove the prefix
				input = input.Substring(Properties.Settings.Default.Prefix.Length);
				//Split the input
				var inputArray = input.Split(new char[] { ' ' }, 2);
				//Get the command
				var cmd = inputArray[0];
				//Get the args
				var args = inputArray.Length > 1 ? inputArray[1] : null;
				//Find the command with the given name
				if (FindCommand(cmd, args))
					return;
				//If no command, give an error message
				Actions.WriteLine("No command could be found with that name.");
			}
		}

		//Search for the correct if statement containing the command's name
		public static bool FindCommand(string cmd, string args)
		{
			//Find what command it belongs to
			if (UICommandNames.GetNamesAndAliases(UICommandEnum.Pause).Contains(cmd, StringComparer.OrdinalIgnoreCase))
			{
				UICommands.PAUSE(args);
			}
			else if (UICommandNames.GetNamesAndAliases(UICommandEnum.BotOwner).Contains(cmd, StringComparer.OrdinalIgnoreCase))
			{
				UICommands.UIGlobalBotOwner(args);
			}
			else if (UICommandNames.GetNamesAndAliases(UICommandEnum.SavePath).Contains(cmd, StringComparer.OrdinalIgnoreCase))
			{
				UICommands.UIGlobalSavePath(args);
			}
			else if (UICommandNames.GetNamesAndAliases(UICommandEnum.Prefix).Contains(cmd, StringComparer.OrdinalIgnoreCase))
			{
				Task.Run(async () =>
				{
					await UICommands.UIGlobalPrefix(args);
				});
			}
			else if (UICommandNames.GetNamesAndAliases(UICommandEnum.Settings).Contains(cmd, StringComparer.OrdinalIgnoreCase))
			{
				UICommands.UIGlobalSettings(args);
			}
			else if (UICommandNames.GetNamesAndAliases(UICommandEnum.BotIcon).Contains(cmd, StringComparer.OrdinalIgnoreCase))
			{
				Task.Run(async () =>
				{
					await UICommands.UIBotIcon(args);
				});
			}
			else if (UICommandNames.GetNamesAndAliases(UICommandEnum.BotGame).Contains(cmd, StringComparer.OrdinalIgnoreCase))
			{
				Task.Run(async () =>
				{
					await UICommands.UIBotGame(args);
				});
			}
			else if (UICommandNames.GetNamesAndAliases(UICommandEnum.BotStream).Contains(cmd, StringComparer.OrdinalIgnoreCase))
			{
				Task.Run(async () =>
				{
					await UICommands.UIBotStream(args);
				});
			}
			else if (UICommandNames.GetNamesAndAliases(UICommandEnum.BotName).Contains(cmd, StringComparer.OrdinalIgnoreCase))
			{
				Task.Run(async () =>
				{
					await UICommands.UIBotName(args);
				});
			}
			else if (UICommandNames.GetNamesAndAliases(UICommandEnum.Disconnect).Contains(cmd, StringComparer.OrdinalIgnoreCase))
			{
				UICommands.UIDisconnect();
			}
			else if (UICommandNames.GetNamesAndAliases(UICommandEnum.Restart).Contains(cmd, StringComparer.OrdinalIgnoreCase))
			{
				UICommands.UIRestart();
			}
			else if (UICommandNames.GetNamesAndAliases(UICommandEnum.ListGuilds).Contains(cmd, StringComparer.OrdinalIgnoreCase))
			{
				UICommands.UIListGuilds();
			}
			else if (UICommandNames.GetNamesAndAliases(UICommandEnum.Shards).Contains(cmd, StringComparer.OrdinalIgnoreCase))
			{
				UICommands.UIModifyShards(args);
			}
			else if (cmd.Equals("test", StringComparison.OrdinalIgnoreCase))
			{
				UICommands.UITest();
			}
			else
			{
				return false;
			}
			return true;
		}
	}

	//Commands the bot can do through the 'console'
	public class UICommands
	{
		//Pause the bot
		public static void PAUSE(string input)
		{
			//Make sure valid input is passed in
			if (input == null)
			{
				Actions.WriteLine("No valid args supplied.");
				return;
			}

			if (input.Equals("on", StringComparison.OrdinalIgnoreCase))
			{
				Actions.WriteLine("Successfully paused the bot.");
				Variables.Pause = true;
			}
			else if (input.Equals("off", StringComparison.OrdinalIgnoreCase))
			{
				Actions.WriteLine("Successfully unpaused the bot.");
				Variables.Pause = false;
			}
			else
			{
				Actions.WriteLine(Constants.ACTION_ERROR);
			}
		}

		//Modify the global prefix
		public static async Task UIGlobalPrefix(string input)
		{
			//Make sure valid input is passed in
			if (input == null)
			{
				Actions.WriteLine("No valid args supplied.");
				return;
			}

			//Get the old prefix
			var oldPrefix = Properties.Settings.Default.Prefix;

			//Check if to clear
			if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
			{
				Properties.Settings.Default.Prefix = Constants.BOT_PREFIX;

				//Send a success message
				Actions.WriteLine("Successfully reset the bot's prefix to `" + Constants.BOT_PREFIX + "`.");
			}
			else
			{
				Properties.Settings.Default.Prefix = input.Trim();

				//Send a success message
				Actions.WriteLine(String.Format("Successfully changed the bot's prefix to `{0}`.", input));
			}

			//Save the settings
			Properties.Settings.Default.Save();
			//Update the game in case it's still the default
			await Actions.SetGame(oldPrefix);
		}

		//Modify the global save path
		public static void UIGlobalSavePath(string input)
		{
			//Make sure valid input is passed in
			if (input == null)
			{
				Actions.WriteLine("No valid args supplied.");
				return;
			}

			//Check if it's current
			else if (input.Equals("current", StringComparison.OrdinalIgnoreCase))
			{
				//Check if the path is empty
				if (String.IsNullOrWhiteSpace(Properties.Settings.Default.Path))
				{
					//If windows then default to appdata
					if (Variables.Windows)
					{
						Actions.WriteLine(String.Format("The current save path is: `{0}`.", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.SERVER_FOLDER)));
					}
					//If not windows then there's no folder
					else
					{
						Actions.WriteLine("There is no save path set.");
					}
				}
				else
				{
					Actions.WriteLine("The current save path is: `" + Properties.Settings.Default.Path + "`.");
				}
				return;
			}
			//See if clear
			else if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
			{
				Properties.Settings.Default.Path = null;
				Properties.Settings.Default.Save();
				Actions.WriteLine("Successfully cleared the current save path.");
				return;
			}
			//See if the directory exists
			else if (!Directory.Exists(input))
			{
				Actions.WriteLine(Actions.ERROR("That directory doesn't exist."));
				return;
			}

			//Set the path
			Properties.Settings.Default.Path = input;
			Properties.Settings.Default.Save();
			Actions.WriteLine(String.Format("Successfully changed the save path to: `{0}`.", input));
		}

		//Modify the global bot owner
		public static void UIGlobalBotOwner(string input)
		{
			//Make sure valid input is passed in
			if (input == null)
			{
				Actions.WriteLine("No valid args supplied.");
				return;
			}

			//Check if it's current
			else if (input.Equals("current", StringComparison.OrdinalIgnoreCase))
			{
				var user = Actions.GetBotOwner(Variables.Client);
				if (user != null)
				{
					Actions.WriteLine(String.Format("The current bot owner is: `{0}`", Actions.FormatUser(user)));
				}
				else
				{
					Actions.WriteLine("This bot is unowned.");
				}
				return;
			}
			//Check if it's clear
			else if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
			{
				Properties.Settings.Default.BotOwner = 0;
				Properties.Settings.Default.Save();
				Actions.WriteLine("Successfully cleared the bot owner.");
				return;
			}
			//Check if there's already a bot owner
			else if (Properties.Settings.Default.BotOwner != 0)
			{
				//Get the bot owner
				var user = Actions.GetBotOwner(Variables.Client);
				Actions.WriteLine(String.Format("There is already a bot owner: `{0}`.", Actions.FormatUser(user)));
				return;
			}

			//Check if ulong
			var ID = Actions.GetUlong(input);
			if (ID == 0)
			{
				Actions.WriteLine("The argument supplied is not a valid number.");
				return;
			}

			//Finally check if it's an actual user
			Discord.IUser globalUser = null;// Variables.Client.GetUser(ID);
			if (globalUser == null)
			{
				Actions.WriteLine("Unable to find any users with that ID.");
				return;
			}

			Properties.Settings.Default.BotOwner = globalUser.Id;
			Properties.Settings.Default.Save();
			Actions.WriteLine(String.Format("Successfully made `{0}` the new bot owner.", Actions.FormatUser(globalUser)));
		}

		//List the global settings or clear them
		public static void UIGlobalSettings(string input)
		{
			//Make sure valid input is passed in
			if (input == null)
			{
				Actions.WriteLine("No valid args supplied.");
				return;
			}

			//Check if current
			else if (input.Equals("current", StringComparison.OrdinalIgnoreCase))
			{
				var description = "";
				description += String.Format("\n\tSave Path: {0}", String.IsNullOrWhiteSpace(Properties.Settings.Default.Path) ? "N/A" : Properties.Settings.Default.Path);
				description += String.Format("\n\tBot Owner ID: {0}", String.IsNullOrWhiteSpace(Properties.Settings.Default.BotOwner.ToString()) ? "N/A" : Properties.Settings.Default.BotOwner.ToString());
				description += String.Format("\n\tStream: {0}", String.IsNullOrWhiteSpace(Properties.Settings.Default.Stream) ? "N/A" : Properties.Settings.Default.Stream);
				Actions.WriteLine(Actions.ReplaceMarkdownChars(description));
			}
			//Check if clear
			else if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
			{
				//Send a success message first instead of after due to the bot losing its ability to do so
				Actions.WriteLine("Successfully cleared all settings. Restarting now...");
				//Reset the settings
				Actions.ResetSettings();
				//Restart the bot
				try
				{
					//Restart the application
					Process.Start(Application.ResourceAssembly.Location);
					//Close the previous version
					Environment.Exit(0);
				}
				catch (Exception)
				{
					Actions.WriteLine("Bot is unable to restart.");
				}
			}
			//Else give action error
			else
			{
				Actions.WriteLine(Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}
		}

		//Change the bot's icon
		public static async Task UIBotIcon(string input)
		{
			//Make sure valid input is passed in
			if (input == null)
			{
				Actions.WriteLine("No valid args supplied.");
				return;
			}

			//See if the user wants to remove the icon
			if (input.Equals("remove", StringComparison.OrdinalIgnoreCase))
			{
				await Variables.Client.GetCurrentUser().ModifyAsync(x => x.Avatar = new Discord.Image());
				Actions.WriteLine("Successfully removed the bot's icon.");
				return;
			}

			//Run separate due to the time it takes
			await Task.Run(async () =>
			{
				//Make sure the input is a valid link
				if (!Actions.ValidateURL(input))
				{
					Actions.WriteLine(Actions.ERROR("Invalid URL."));
					return;
				}

				//Check the image's file size first
				var req = HttpWebRequest.Create(input);
				req.Method = "HEAD";
				try
				{
					using (WebResponse resp = req.GetResponse())
					{
						int ContentLength = 0;
						if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
						{
							//Check if valid content type
							if (!Constants.VALIDIMAGEEXTENSIONS.Contains("." + resp.Headers.Get("Content-Type").Split('/').Last()))
							{
								Actions.WriteLine(Actions.ERROR("Image must be a png or jpg."));
								return;
							}
							else
							{
								if (ContentLength > 2500000)
								{
									//Check if bigger than 2.5MB
									Actions.WriteLine(Actions.ERROR("Image is bigger than 2.5MB. Please manually upload instead."));
									return;
								}
								else if (ContentLength == 0)
								{
									//Check if nothing was gotten
									Actions.WriteLine(Actions.ERROR("Unable to get the image's file size."));
									return;
								}
							}
						}
						else
						{
							Actions.WriteLine(Actions.ERROR("Unable to get the image's file size."));
							return;
						}
					}
				}
				catch
				{
					Actions.WriteLine(Actions.ERROR("Unable to create webrequest with URL."));
					return;
				}

				//Send a message saying how it's progressing
				Actions.WriteLine("Attempting to download the file...");

				//Set the name of the file to prevent typos between the three places that use it
				var path = Actions.GetServerFilePath(0, "boticon" + Path.GetExtension(input).ToLower(), true);

				//Download the image
				using (var webclient = new WebClient())
				{
					webclient.DownloadFile(input, path);
				}

				//Create a second filestream to upload the image
				using (FileStream imgStream = new FileStream(path, FileMode.Open, FileAccess.Read))
				{
					await Variables.Client.GetCurrentUser().ModifyAsync(x => x.Avatar = new Discord.Image(imgStream));
				}

				//Delete the file and send a success message
				File.Delete(path);
				Actions.WriteLine("Successfully changed the bot's icon.");
			});
		}

		//Change the bot's game
		public static async Task UIBotGame(string input)
		{
			//Check the game name length
			if (input.Length > Constants.GAME_MAX_LENGTH)
			{
				Actions.WriteLine(Actions.ERROR("Game name cannot be longer than 128 characters or else it doesn't show to other people."));
				return;
			}

			//Save the game as a setting
			Properties.Settings.Default.Game = input;
			Properties.Settings.Default.Save();

			await Variables.Client.SetGameAsync(input, Variables.Client.GetCurrentUser().Game.Value.StreamUrl, Variables.Client.GetCurrentUser().Game.Value.StreamType);
			Actions.WriteLine(String.Format("Game set to `{0}`.", input));
		}

		//Change the bot's stream
		public static async Task UIBotStream(string input)
		{
			//If empty string, take that as the notion to turn the stream off
			if (!String.IsNullOrWhiteSpace(input))
			{
				//Check if it's an actual stream
				if (!input.StartsWith(Constants.STREAM_URL, StringComparison.OrdinalIgnoreCase))
				{
					Actions.WriteLine(Actions.ERROR("Link must be from Twitch.TV."));
					return;
				}
				else if (input.Substring(Constants.STREAM_URL.Length).Contains('/'))
				{
					Actions.WriteLine(Actions.ERROR("Link must be to a user's stream."));
					return;
				}
			}

			//Save the stream as a setting
			Properties.Settings.Default.Stream = input;
			Properties.Settings.Default.Save();

			//Check if to turn off the streaming
			var streamType = Discord.StreamType.Twitch;
			if (input == null)
			{
				streamType = Discord.StreamType.NotStreaming;
			}

			//Set the stream
			await Variables.Client.SetGameAsync(Variables.Client.GetCurrentUser().Game.Value.Name, input, streamType);
			Actions.WriteLine(String.Format("Successfully {0} the bot's stream{1}.", input == null ? "reset" : "set", input == null ? "" : " to `" + input + "`"));
		}

		//Change the bot's name
		public static async Task UIBotName(string input)
		{
			//Make sure valid input is passed in
			if (input == null)
			{
				Actions.WriteLine("No valid args supplied.");
				return;
			}

			//Names have the same length requirements as nicknames
			if (input.Length > Constants.NICKNAME_MAX_LENGTH)
			{
				Actions.WriteLine(Actions.ERROR("Name cannot be more than 32 characters."));
				return;
			}
			else if (input.Length < Constants.NICKNAME_MIN_LENGTH)
			{
				Actions.WriteLine(Actions.ERROR("Name cannot be less than 2 characters."));
				return;
			}

			//Change the bots name to it
			await Variables.Client.GetCurrentUser().ModifyAsync(x => x.Username = input);

			//Send a success message
			Actions.WriteLine(String.Format("Successfully changed my username to `{0}`.", input));
		}

		//Disconnect the bot
		public static void UIDisconnect()
		{
			Environment.Exit(0);
		}

		//Restart the bot
		public static void UIRestart()
		{
			try
			{
				//Restart the application
				Process.Start(Application.ResourceAssembly.Location);
				//Close the previous version
				Environment.Exit(0);
			}
			catch (Exception)
			{
				Actions.WriteLine("Bot is unable to restart.");
			}
		}

		//List all the guilds the bot's in
		public static void UIListGuilds()
		{
			//Go through each guild and add them to the list
			int count = 1;
			var guildStrings = Variables.Client.GetGuilds().ToList().Select(x => String.Format("{0}. {1} Owner: {2}", count++.ToString("00"), Actions.FormatGuild(x), Actions.FormatUser(x.Owner)));

			//Send it to have the hyperlink created and go to the output window
			BotWindow.AddHyperlink(BotWindow.MainOutput, Actions.UploadToHastebin(String.Join("\n", guildStrings)), "Listed Guilds");
		}

		//Change the amount of shards the bot has
		public static void UIModifyShards(string input)
		{
			//Make sure valid input is passed in
			if (input == null)
			{
				Actions.WriteLine("No valid args supplied.");
				return;
			}

			//Check if valid number
			var number = 0;
			if (!int.TryParse(input, out number))
			{
				Actions.WriteLine("Invalid input for number.");
				return;
			}

			//Check if the client has too many servers for that to work
			if (Variables.Client.GetGuilds().Count >= number * 2500)
			{
				Actions.WriteLine("With the current amount of guilds the client has, the minimum shard number is: " + Variables.Client.GetGuilds().Count / 2500 + 1);
				return;
			}

			//Set and save the amount
			Properties.Settings.Default.ShardCount = number;
			Properties.Settings.Default.Save();

			//Send a success message
			Actions.WriteLine(String.Format("Successfully set the shard amount to {0}.", number));
		}

		//Test command
		public static void UITest()
		{
			//Actions.WriteLine(Variables.Client.Latency.ToString());
		}
	}

	//Write the console output into the UI
	public class TextBoxStreamWriter : TextWriter 
	{
		private TextBoxBase mOutput;
		private bool mIgnoreNewLines;

		public TextBoxStreamWriter(TextBoxBase output)
		{
			mOutput = output;
			mIgnoreNewLines = output is RichTextBox;
		}

		public override void Write(char value)
		{
			if (mIgnoreNewLines && value.Equals('\n'))
				return;

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

	//Resize font
	public class FontResizer : IValueConverter
	{
		double convertFactor;
		public FontResizer(double convertFactor)
		{
			this.convertFactor = convertFactor;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Math.Max((int)(System.Convert.ToDouble(value) * convertFactor), -1);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
