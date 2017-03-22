using ICSharpCode.AvalonEdit;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Advobot
{
	//Create the UI
	public class BotWindow : Window
	{
		private static Grid mLayout = new Grid();

		#region Input
		private static Grid mInputLayout = new Grid();
		//(Max height has to be set here as a large number to a) not get in the way and b) not crash when resized small) I don't really like using a RTB for the input.
		private static TextBox mInputBox = new TextBox { MaxLength = 250, MaxLines = 5, MaxHeight = 1000, TextWrapping = TextWrapping.Wrap };
		private static Button mInputButton = new Button { IsEnabled = false, Content = "Enter", };
		#endregion

		#region Edit
		private static Grid mEditLayout = new Grid { Visibility = Visibility.Collapsed, };
		private static Grid mEditButtonLayout = new Grid();
		private static TextEditor mEditBox = new TextEditor { WordWrap = true, VerticalScrollBarVisibility = ScrollBarVisibility.Visible, ShowLineNumbers = true, };
		private static TextBox mEditSaveBox = new TextBox { Text = "Successfully saved the file.", Visibility = Visibility.Collapsed, TextAlignment = TextAlignment.Center };
		private static Button mEditSaveButton = new Button { Content = "Save", };
		private static Button mEditCloseButton = new Button { Content = "Close", };
		#endregion

		#region Output
		private static MenuItem mOutputContextMenuSave = new MenuItem { Header = "Save Output Log", };
		private static MenuItem mOutputContextMenuClear = new MenuItem { Header = "Clear Output Log", };
		private static ContextMenu mOutputContextMenu = new ContextMenu { ItemsSource = new[] { mOutputContextMenuSave, mOutputContextMenuClear }, };
		private static RichTextBox mOutputBox = new RichTextBox
		{
			ContextMenu = mOutputContextMenu,
			IsReadOnly = true,
			IsDocumentEnabled = true,
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			Background = Brushes.White,
		};		
		#endregion

		#region Menu
		private const string mHelpSynt = "Command Syntax:\n\t[] means required\n\t<> means optional\n\t| means or";
		private const string mHelpInf1 = "\n\nLatency:\n\tTime it takes for a command to reach the bot.\nMemory:\n\tAmount of RAM the program is using.\n\t(This is wrong most of the time.)";
		private const string mHelpInf2 = "\nThreads:\n\tWhere all the actions in the bot happen.\nShards:\n\tHold all the guilds a bot has on its client.\n\tThere is a limit of 2500 guilds per shard.";
		private const string mHelpVers = "\n\nAPI Wrapper Version: " + Constants.API_VERSION + "\nBot Version: " + Constants.BOT_VERSION + "\nGitHub Repository: ";
		private const string mHelpHelp = "\n\nNeed additional help? Join the Discord server: ";
		private static readonly string mCmdsCmds = "Commands:".PadRight(Constants.PAD_RIGHT) + "Aliases:\n" + UICommandNames.FormatStringForUse();
		private static Inline mHelpFirstRun = new Run(mHelpSynt + mHelpInf1 + mHelpInf2 + mHelpVers);
		private static Inline mHelpFirstHyperlink = UIMakeElement.MakeHyperlink("https://github.com/advorange/Advobot", "Advobot");
		private static Inline mHelpSecondRun = new Run(mHelpHelp);
		private static Inline mHelpSecondHyperlink = UIMakeElement.MakeHyperlink("https://www.discord.gg/ad", "Here");
		private static Inline mCmdsFirstRun = new Run(mCmdsCmds);
		private static Inline mInfoFirstRun = new Run(Actions.FormatLoggedThings());
		private static TreeView mFileTreeView = new TreeView();
		private static Paragraph mFirstParagraph = new Paragraph(mHelpFirstRun);
		private static Paragraph mSecondParagraph = new Paragraph(mCmdsFirstRun);
		private static Paragraph mThirdParagraph = new Paragraph(mInfoFirstRun);
		private static Paragraph mFourthParagraph = new Paragraph();
		private static RichTextBox mMenuBox = new RichTextBox { IsReadOnly = true, IsDocumentEnabled = true, Visibility = Visibility.Collapsed, Background = Brushes.White, };
		private const string mFirstButtonString = "Help";
		private const string mSecondButtonString = "Commands";
		private const string mThirdButtonString = "Info";
		private const string mFourthButtonString = "Files";
		private static string mLastButtonClicked;
		private static Grid mButtonLayout = new Grid();
		private static Button mFirstButton = new Button { Content = mFirstButtonString, };
		private static Button mSecondButton = new Button { Content = mSecondButtonString, };
		private static Button mThirdButton = new Button { Content = mThirdButtonString, };
		private static Button mFourthButton = new Button { Content = mFourthButtonString, };
		#endregion

		#region System Info
		//Layout
		private static Grid mSysInfoLayout = new Grid();
		private static TextBox mSysInfoUnder = new TextBox();
		//Textboxes
		private static TextBox mLatency = new TextBox { IsReadOnly = true, BorderBrush = Brushes.Transparent, };
		private static TextBox mMemory = new TextBox { IsReadOnly = true, BorderBrush = Brushes.Transparent, };
		private static TextBox mThreads = new TextBox { IsReadOnly = true, BorderBrush = Brushes.Transparent, };
		private static TextBox mShards = new TextBox { IsReadOnly = true, BorderBrush = Brushes.Transparent, };
		private static TextBox mPrefix = new TextBox { IsReadOnly = true, BorderBrush = Brushes.Transparent, };
		//Viewboxes
		private static Viewbox mLatencyView = new Viewbox { Child = mLatency, };
		private static Viewbox mMemoryView = new Viewbox { Child = mMemory, };
		private static Viewbox mThreadsView = new Viewbox { Child = mThreads, };
		private static Viewbox mShardsView = new Viewbox { Child = mShards, };
		private static Viewbox mPrefixView = new Viewbox { Child = mPrefix, HorizontalAlignment = HorizontalAlignment.Stretch, };
		#endregion

		#region Misc
		private static ToolTip mMemHoverInfo = new ToolTip { Content = "This is not guaranteed to be 100% correct.", };
		private static ToolTip mSaveToolTip = new ToolTip() { Content = "Successfully saved the file." };
		private static Binding mInputBinding = UILayoutModification.CreateBinding(.275);
		private static Binding mFirstMenuBinding = UILayoutModification.CreateBinding(.0157);
		private static Binding mSecondMenuBinding = UILayoutModification.CreateBinding(.0195);
		#endregion

		//Create the bot window
		public BotWindow()
		{
			FontFamily = new FontFamily("Courier New");
			InitializeComponent();
		}
		//Create all the components
		private void InitializeComponent()
		{
			//Main layout
			UILayoutModification.AddRows(mLayout, 100);
			UILayoutModification.AddCols(mLayout, 4);

			//Output
			UILayoutModification.AddItemAndSetPositionsAndSpans(mLayout, mOutputBox, 0, 87, 0, 4);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mLayout, mMenuBox, 0, 90, 3, 1);

			//System Info
			UILayoutModification.AddItemAndSetPositionsAndSpans(mLayout, mSysInfoLayout, 87, 3, 0, 3, 0, 5);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mSysInfoLayout, mSysInfoUnder, 0, 1, 0, 5);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mSysInfoLayout, mLatencyView, 0, 1, 0, 1);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mSysInfoLayout, mMemoryView, 0, 1, 1, 1);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mSysInfoLayout, mThreadsView, 0, 1, 2, 1);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mSysInfoLayout, mShardsView, 0, 1, 3, 1);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mSysInfoLayout, mPrefixView, 0, 1, 4, 1);

			//Input
			UILayoutModification.AddItemAndSetPositionsAndSpans(mLayout, mInputLayout, 90, 10, 0, 3, 1, 10);
			UILayoutModification.AddBinding(mInputBox, FontSizeProperty, mInputBinding);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mInputLayout, mInputBox, 0, 1, 0, 9);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mInputLayout, mInputButton, 0, 1, 9, 1);

			//Buttons
			UILayoutModification.AddItemAndSetPositionsAndSpans(mLayout, mButtonLayout, 87, 13, 3, 1, 1, 4);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mButtonLayout, mFirstButton, 0, 1, 0, 1);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mButtonLayout, mSecondButton, 0, 1, 1, 1);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mButtonLayout, mThirdButton, 0, 1, 2, 1);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mButtonLayout, mFourthButton, 0, 1, 3, 1);

			//Edit
			UILayoutModification.AddItemAndSetPositionsAndSpans(mLayout, mEditLayout, 0, 100, 0, 4, 100, 4);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mEditLayout, mEditBox, 0, 100, 0, 3);
			UILayoutModification.AddBinding(mEditBox, FontSizeProperty, mSecondMenuBinding);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mEditLayout, mEditSaveBox, 85, 2, 3, 1);
			UILayoutModification.AddBinding(mEditSaveBox, FontSizeProperty, mFirstMenuBinding);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mEditLayout, mEditButtonLayout, 87, 13, 3, 1, 1, 2);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mEditButtonLayout, mEditSaveButton, 0, 1, 0, 1);
			UILayoutModification.AddItemAndSetPositionsAndSpans(mEditButtonLayout, mEditCloseButton, 0, 1, 1, 1);

			//Paragraphs
			mFirstParagraph.Inlines.AddRange(new Inline[] { mHelpFirstHyperlink, mHelpSecondRun, mHelpSecondHyperlink });
			mFourthParagraph.Inlines.Add(mFileTreeView);

			//Events
			mInputBox.KeyUp += AcceptInput;
			mMemory.MouseEnter += ModifyMemHoverInfo;
			mMemory.MouseLeave += ModifyMemHoverInfo;
			mInputButton.Click += AcceptInput;
			mOutputContextMenuSave.Click += SaveOutput;
			mOutputContextMenuClear.Click += ClearOutput;
			mFirstButton.Click += BringUpMenu;
			mSecondButton.Click += BringUpMenu;
			mThirdButton.Click += BringUpMenu;
			mFourthButton.Click += BringUpMenu;
			mEditCloseButton.Click += CloseEditScreen;
			mEditSaveButton.Click += SaveEditScreen;

			//Set this panel as the content for this window.
			Content = mLayout;
			//Actually run the application
			RunApplication();
		}
		//Actually make the application work
		private void RunApplication()
		{
			//Make console output show on the output text block and box
			Console.SetOut(new UITextBoxStreamWriter(mOutputBox));

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
				mThirdParagraph.Inlines.Clear();
				mThirdParagraph.Inlines.Add(new Run(Actions.FormatLoggedThings() + "\n\nCharacter Count: ~525,000\nLine Count: ~15,000"));
			};
			//Make the timer update every so often
			timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
			//Start the timer
			timer.Start();
		}

		//Accept input through the enter key
		public void AcceptInput(object sender, KeyEventArgs e)
		{
			var text = mInputBox.Text;
			if (String.IsNullOrWhiteSpace(text))
			{
				mInputButton.IsEnabled = false;
				return;
			}
			else
			{
				if (e.Key.Equals(Key.Enter) || e.Key.Equals(Key.Return))
				{
					UICommandHandler.GatherInput();
				}
				else
				{
					mInputButton.IsEnabled = true;
				}
			}
		}
		//Accept input through the enter button
		public void AcceptInput(object sender, RoutedEventArgs e)
		{
			UICommandHandler.GatherInput();
		}
		//Save the output after the button is clicked
		public void SaveOutput(object sender, RoutedEventArgs e)
		{
			//Make sure the path is valid
			var path = Actions.GetDirectory("Output_Log_" + DateTime.UtcNow.ToString("MM-dd_HH-mm-ss") + Constants.FILE_EXTENSION);
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
				new TextRange(mOutputBox.Document.ContentStart, mOutputBox.Document.ContentEnd).Save(stream, DataFormats.Text, true);
			}

			//Write to the console telling the user that the console log was successfully saved
			Actions.WriteLine("Successfully saved the output log.");
		}
		//Clear the output after the button is clicked
		public void ClearOutput(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show("Are you sure you want to clear the output window?", Variables.Bot_Name, MessageBoxButton.OKCancel);

			switch (result)
			{
				case MessageBoxResult.OK:
				{
					mOutputBox.Document.Blocks.Clear();
					break;
				}
			}
		}
		//Bring up the associated menu
		public void BringUpMenu(object sender, RoutedEventArgs e)
		{
			//Make sure everything is loaded first
			if (!Variables.Loaded)
				return;
			//Get the button's name
			var name = (sender as Button).Content.ToString();
			//Remove the current blocks in the document
			mMenuBox.Document.Blocks.Clear();
			//Disable the rtb if the most recent button clicked is clicked again
			if (Actions.CaseInsEquals(name, mLastButtonClicked))
			{
				UILayoutModification.SetColAndSpan(mOutputBox, 0, 4);
				mMenuBox.Visibility = Visibility.Collapsed;
				mLastButtonClicked = null;
			}
			else
			{
				//Resize the regular output window
				UILayoutModification.SetColAndSpan(mOutputBox, 0, 3);
				//Make the secondary output visible
				mMenuBox.Visibility = Visibility.Visible;
				//Keep track of the last button clicked
				mLastButtonClicked = name;

				//Show the text for help
				if (Actions.CaseInsEquals(name, mFirstButtonString))
				{
					mMenuBox.SetBinding(FontSizeProperty, mFirstMenuBinding);
					mMenuBox.Document.Blocks.Add(mFirstParagraph);
				}
				//Show the text for commands
				else if (Actions.CaseInsEquals(name, mSecondButtonString))
				{
					mMenuBox.SetBinding(FontSizeProperty, mSecondMenuBinding);
					mMenuBox.Document.Blocks.Add(mSecondParagraph);
				}
				//Show the text for info
				else if (Actions.CaseInsEquals(name, mThirdButtonString))
				{
					mMenuBox.SetBinding(FontSizeProperty, mSecondMenuBinding);
					mMenuBox.Document.Blocks.Add(mThirdParagraph);
				}
				//Show the text for settings
				else if (Actions.CaseInsEquals(name, mFourthButtonString))
				{
					mFourthParagraph = UIMakeElement.MakeGuildTreeView(mFourthParagraph);
					mMenuBox.SetBinding(FontSizeProperty, mSecondMenuBinding);
					mMenuBox.Document.Blocks.Add(mFourthParagraph);
				}
			}
		}
		//Modify the memory tooltip
		public void ModifyMemHoverInfo(object sender, MouseEventArgs e)
		{
			UILayoutModification.ToggleToolTip(mMemHoverInfo);
		}
		//Bring up the edit window
		public static void TreeViewDoubleClick(object sender, RoutedEventArgs e)
		{
			//Get the double clicked item
			var treeItem = sender as TreeViewItem;
			if (treeItem == null)
				return;
			//Get the path from the tag
			var fileLocation = treeItem.Tag.ToString();
			if (fileLocation == null)
				return;
			//Print out all the info in that file
			var data = "";
			using (var reader = new StreamReader(fileLocation))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					data += line + Environment.NewLine;
				}
			}
			//Change the text in the bot and make it visible
			mEditBox.Text = data;
			mEditBox.Tag = fileLocation;
			mEditLayout.Visibility = Visibility.Visible;
		}
		//Close the edit screen
		public static void CloseEditScreen(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show("Are you sure you want to close the edit window?", Variables.Bot_Name, MessageBoxButton.OKCancel);

			switch (result)
			{
				case MessageBoxResult.OK:
				{
					mEditLayout.Visibility = Visibility.Collapsed;
					break;
				}
			}
		}
		//Save the rewritten input in the save screen
		public static void SaveEditScreen(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show("Are you certain this document has not been formatted in a way that breaks it?", Variables.Bot_Name, MessageBoxButton.OKCancel);

			switch (result)
			{
				case MessageBoxResult.OK:
				{
					//Rewrite the input back in
					var fileLocation = mEditBox.Tag.ToString();
					//Make sure the file location exists
					if (String.IsNullOrWhiteSpace(fileLocation) || !File.Exists(fileLocation))
					{
						MessageBox.Show("Unable to gather the path for this file. This file has NOT been saved.", Variables.Bot_Name);
					}
					else
					{
						//Save the file
						using (var writer = new StreamWriter(fileLocation))
						{
							writer.WriteLine(mEditBox.Text);
						}
						//Give a notification saying it was saved
						mSaveToolTip.Dispatcher.InvokeAsync(async () =>
						{
							UILayoutModification.ToggleUIElement(mEditSaveBox);
							await Task.Delay(2500);
							UILayoutModification.ToggleUIElement(mEditSaveBox);
						});
					}
					break;
				}
			}
		}

		public static RichTextBox Output { get { return mOutputBox; } }
		public static RichTextBox Menu { get { return mMenuBox; } }
		public static TextBox Input { get { return mInputBox; } }
		public static Button InputButton { get { return mInputButton; } }
	}

	//Modify the UI
	public class UILayoutModification
	{
		//Add in X amount of rows
		public static void AddRows(Grid grid, int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				grid.RowDefinitions.Add(new RowDefinition());
			}
		}

		//Add in X amount of columns
		public static void AddCols(Grid grid, int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				grid.ColumnDefinitions.Add(new ColumnDefinition());
			}
		}

		//Set the row and row span
		public static void SetRowAndSpan(UIElement item, int start = 0, int length = 1)
		{
			Grid.SetRow(item, start < 0 ? 0 : start);
			Grid.SetRowSpan(item, length < 1 ? 1 : length);
		}

		//Set the colum and column span
		public static void SetColAndSpan(UIElement item, int start = 0, int length = 1)
		{
			Grid.SetColumn(item, start < 0 ? 0 : start);
			Grid.SetColumnSpan(item, length < 1 ? 1 : length);
		}

		//Add the child to the given grid with specified starting rows/columns and lengths
		public static void AddItemAndSetPositionsAndSpans(Panel parent, UIElement child, int rowStart, int rowLength, int columnStart, int columnLength, int setRows = 0, int setColumns = 0)
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

		//Create a new binding
		public static Binding CreateBinding(double val)
		{
			return new Binding
			{
				Path = new PropertyPath("ActualHeight"),
				RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1),
				Converter = new UIFontResizer(val),
			};
		}

		//Add a binding to an element
		public static void AddBinding(Control element, DependencyProperty dproperty, Binding binding)
		{
			element.SetBinding(dproperty, binding);
		}

		//Flip a tool tip
		public static void ToggleToolTip(ToolTip ttip)
		{
			ttip.IsOpen = !ttip.IsOpen;
		}

		//Flip the visibility of a UI object
		public static void ToggleUIElement(UIElement ele)
		{
			ele.Visibility = ele.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
		}

		//Add a hypderlink to an output box
		public static void AddHyperlink(RichTextBox output, string link, string name, string beforeText = null, string afterText = null)
		{
			//Create the hyperlink
			var hyperlink = UIMakeElement.MakeHyperlink(link, name);
			if (hyperlink == null)
			{
				return;
			}
			//Check if the paragraph is valid
			var para = BotWindow.Output.Document.Blocks.LastBlock as Paragraph;
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
	}

	//Make certain elements
	public class UIMakeElement
	{
		//Make a hyperlink
		public static Hyperlink MakeHyperlink(string link, string name)
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

		//Make the tree view for the guilds on the bot
		public static Paragraph MakeGuildTreeView(Paragraph input)
		{
			//Get the directory
			var directory = Actions.GetDirectory();
			if (directory == null || !Directory.Exists(directory))
				return input;

			//Create the treeview
			var treeView = new TreeView() { BorderThickness = new Thickness(0) };

			//Format the treeviewitems
			Directory.GetDirectories(directory).ToList().ForEach(guildDir =>
			{
				//Separate the ID from the rest of the directory
				var strID = guildDir.Substring(guildDir.LastIndexOf('\\') + 1);
				//Make sure the ID is valid
				if (!ulong.TryParse(strID, out ulong ID))
					return;

				//Create the guild's treeviewitem
				TreeViewItem guildItem;
				try
				{
					guildItem = new TreeViewItem() { Header = String.Format("({0}) {1}", strID, Variables.Client.GetGuild(ID).Name) };
				}
				catch
				{
					//This means that the guild is currently not using the bot. Don't delete the directory in case they ever do come back to using the bot.
					return;
				}

				//Add in all of the files the guild has
				Directory.GetFiles(guildDir).ToList().ForEach(file =>
				{
					//Get the file with its extension
					var fileAndExtension = file.Substring(file.LastIndexOf('\\') + 1);
					//Check if the gotten file is a valid file
					if (!Actions.CaseInsContains(Constants.VALID_GUILD_FILES, fileAndExtension))
						return;
					//Create the item
					var fileItem = new TreeViewItem() { Header = fileAndExtension.Split('.')[0], Tag = file };
					//Add in the double click event
					fileItem.MouseDoubleClick += BotWindow.TreeViewDoubleClick;
					//Add it to the guild item
					guildItem.Items.Add(fileItem);
				});
				//Create a new treeview item with all of these items
				treeView.Items.Add(guildItem);
			});

			//Remove all current information in the paragraph
			input.Inlines.Clear();
			//Add back in the new treeview
			input.Inlines.Add(treeView);

			return input;
		}
	}

	//New class to handle commands
	public class UICommandHandler
	{
		//Gather the input and reset the input
		public static void GatherInput()
		{
			//Get the current text
			var text = BotWindow.Input.Text.Trim(new char[] { '\r', '\n' });
			BotWindow.Input.Text = "";
			BotWindow.InputButton.IsEnabled = false;
			//Write it out to the ghetto console
			Console.WriteLine(text);
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
				HandleCommand(text);
			}
		}

		//Pull out the cmd name and the args
		public static void HandleCommand(string input)
		{
			//Check if it's a global bot command done through the console
			if (Actions.CaseInsStartsWith(input, Properties.Settings.Default.Prefix))
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
			if (Actions.CaseInsContains(UICommandNames.GetNameAndAliases(UICommandEnum.Pause), cmd))
			{
				UICommands.PAUSE(args);
			}
			else if (Actions.CaseInsContains(UICommandNames.GetNameAndAliases(UICommandEnum.BotOwner), cmd))
			{
				UICommands.UIGlobalBotOwner(args);
			}
			else if (Actions.CaseInsContains(UICommandNames.GetNameAndAliases(UICommandEnum.SavePath), cmd))
			{
				UICommands.UIGlobalSavePath(args);
			}
			else if (Actions.CaseInsContains(UICommandNames.GetNameAndAliases(UICommandEnum.Prefix), cmd))
			{
				Task.Run(async () =>
				{
					await UICommands.UIGlobalPrefix(args);
				});
			}
			else if (Actions.CaseInsContains(UICommandNames.GetNameAndAliases(UICommandEnum.Settings), cmd))
			{
				UICommands.UIGlobalSettings(args);
			}
			else if (Actions.CaseInsContains(UICommandNames.GetNameAndAliases(UICommandEnum.BotIcon), cmd))
			{
				Task.Run(async () =>
				{
					await UICommands.UIBotIcon(args);
				});
			}
			else if (Actions.CaseInsContains(UICommandNames.GetNameAndAliases(UICommandEnum.BotGame), cmd))
			{
				Task.Run(async () =>
				{
					await UICommands.UIBotGame(args);
				});
			}
			else if (Actions.CaseInsContains(UICommandNames.GetNameAndAliases(UICommandEnum.BotStream), cmd))
			{
				Task.Run(async () =>
				{
					await UICommands.UIBotStream(args);
				});
			}
			else if (Actions.CaseInsContains(UICommandNames.GetNameAndAliases(UICommandEnum.BotName), cmd))
			{
				Task.Run(async () =>
				{
					await UICommands.UIBotName(args);
				});
			}
			else if (Actions.CaseInsContains(UICommandNames.GetNameAndAliases(UICommandEnum.Disconnect), cmd))
			{
				UICommands.UIDisconnect();
			}
			else if (Actions.CaseInsContains(UICommandNames.GetNameAndAliases(UICommandEnum.Restart), cmd))
			{
				UICommands.UIRestart();
			}
			else if (Actions.CaseInsContains(UICommandNames.GetNameAndAliases(UICommandEnum.ListGuilds), cmd))
			{
				UICommands.UIListGuilds();
			}
			else if (Actions.CaseInsContains(UICommandNames.GetNameAndAliases(UICommandEnum.Shards), cmd))
			{
				UICommands.UIModifyShards(args);
			}
			else if (Actions.CaseInsEquals(cmd, "test"))
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

			if (Actions.CaseInsEquals(input, "on"))
			{
				Actions.WriteLine("Successfully paused the bot.");
				Variables.Pause = true;
			}
			else if (Actions.CaseInsEquals(input, "off"))
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
			if (Actions.CaseInsEquals(input, "clear"))
			{
				Properties.Settings.Default.Prefix = Constants.BOT_PREFIX;

				//Send a success message
				Actions.WriteLine(String.Format("Successfully reset the bot's prefix to '{0}'.", Constants.BOT_PREFIX));
			}
			else
			{
				Properties.Settings.Default.Prefix = input.Trim();

				//Send a success message
				Actions.WriteLine(String.Format("Successfully changed the bot's prefix to '{0}'.", input));
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
			else if (Actions.CaseInsEquals(input, "current"))
			{
				//Check if the path is empty
				if (String.IsNullOrWhiteSpace(Properties.Settings.Default.Path))
				{
					//If windows then default to appdata
					if (Variables.Windows)
					{
						Actions.WriteLine(String.Format("The current save path is: '{0}'.", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.SERVER_FOLDER)));
					}
					//If not windows then there's no folder
					else
					{
						Actions.WriteLine("There is no save path set.");
					}
				}
				else
				{
					Actions.WriteLine(String.Format("The current save path is: '{0}'.", Properties.Settings.Default.Path));
				}
				return;
			}
			//See if clear
			else if (Actions.CaseInsEquals(input, "clear"))
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
			Actions.WriteLine(String.Format("Successfully changed the save path to: '{0}'.", input));
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
			else if (Actions.CaseInsEquals(input, "current"))
			{
				var user = Actions.GetBotOwner(Variables.Client);
				if (user != null)
				{
					Actions.WriteLine(String.Format("The current bot owner is: '{0}'", Actions.FormatUser(user)));
				}
				else
				{
					Actions.WriteLine("This bot is unowned.");
				}
				return;
			}
			//Check if it's clear
			else if (Actions.CaseInsEquals(input, "clear"))
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
				Actions.WriteLine(String.Format("There is already a bot owner: '{0}'.", Actions.FormatUser(Actions.GetBotOwner(Variables.Client))));
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
			Discord.IUser globalUser = Variables.Client.GetUser(ID);
			if (globalUser == null)
			{
				Actions.WriteLine("Unable to find any users with that ID.");
				return;
			}

			Properties.Settings.Default.BotOwner = globalUser.Id;
			Properties.Settings.Default.Save();
			Actions.WriteLine(String.Format("Successfully made '{0}' the new bot owner.", Actions.FormatUser(globalUser)));
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
			else if (Actions.CaseInsEquals(input, "current"))
			{
				var description = "";
				description += String.Format("\n\tSave Path: {0}", String.IsNullOrWhiteSpace(Properties.Settings.Default.Path) ? "N/A" : Properties.Settings.Default.Path);
				description += String.Format("\n\tBot Owner ID: {0}", String.IsNullOrWhiteSpace(Properties.Settings.Default.BotOwner.ToString()) ? "N/A" : Properties.Settings.Default.BotOwner.ToString());
				description += String.Format("\n\tStream: {0}", String.IsNullOrWhiteSpace(Properties.Settings.Default.Stream) ? "N/A" : Properties.Settings.Default.Stream);
				Actions.WriteLine(Actions.ReplaceMarkdownChars(description));
			}
			//Check if clear
			else if (Actions.CaseInsEquals(input, "clear"))
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
			if (Actions.CaseInsEquals(input, "remove"))
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
						if (int.TryParse(resp.Headers.Get("Content-Length"), out int ContentLength))
						{
							//Check if valid content type
							if (!Constants.VALID_IMAGE_EXTENSIONS.Contains("." + resp.Headers.Get("Content-Type").Split('/').Last()))
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
				var path = Actions.GetDirectory("boticon" + Path.GetExtension(input).ToLower());

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
				Actions.WriteLine(Actions.ERROR(String.Format("Game name cannot be longer than '{0}' characters or else it doesn't show to other people.", Constants.GAME_MAX_LENGTH)));
				return;
			}

			//Save the game as a setting
			Properties.Settings.Default.Game = input;
			Properties.Settings.Default.Save();

			await Variables.Client.SetGameAsync(input, Variables.Client.GetCurrentUser().Game.Value.StreamUrl, Variables.Client.GetCurrentUser().Game.Value.StreamType);
			Actions.WriteLine(String.Format("Game set to '{0}'.", input));
		}

		//Change the bot's stream
		public static async Task UIBotStream(string input)
		{
			//If empty string, take that as the notion to turn the stream off
			if (!String.IsNullOrWhiteSpace(input))
			{
				//Check if it's an actual stream
				if (!Actions.CaseInsStartsWith(input, Constants.STREAM_URL))
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
			var streamType = input == null ? Discord.StreamType.NotStreaming : Discord.StreamType.Twitch;

			//Set the stream
			await Variables.Client.SetGameAsync(Variables.Client.GetCurrentUser().Game.Value.Name, input, streamType);
			Actions.WriteLine(String.Format("Successfully {0} the bot's stream{1}.", input == null ? "reset" : "set", input == null ? "" : " to '" + input + "'"));
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
				Actions.WriteLine(Actions.ERROR(String.Format("Name cannot be more than '{0}' characters.", Constants.NICKNAME_MAX_LENGTH)));
				return;
			}
			else if (input.Length < Constants.NICKNAME_MIN_LENGTH)
			{
				Actions.WriteLine(Actions.ERROR(String.Format("Name cannot be less than '{0}' characters.", Constants.NICKNAME_MIN_LENGTH)));
				return;
			}

			//Change the bots name to it
			await Variables.Client.GetCurrentUser().ModifyAsync(x => x.Username = input);

			//Send a success message
			Actions.WriteLine(String.Format("Successfully changed my username to '{0}'.", input));
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

			//Get the URL
			Actions.TryToUploadToHastebin(String.Join("\n", guildStrings), out string url);

			//Send it to have the hyperlink created and go to the output window
			UILayoutModification.AddHyperlink(BotWindow.Output, url, "Listed Guilds");
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
			if (!int.TryParse(input, out int number))
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
	public class UITextBoxStreamWriter : TextWriter 
	{
		private TextBoxBase mOutput;
		private bool mIgnoreNewLines;

		public UITextBoxStreamWriter(TextBoxBase output)
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
	public class UIFontResizer : IValueConverter
	{
		double convertFactor;
		public UIFontResizer(double convertFactor)
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
