using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

		//Output textbox
		private static RichTextBox mOutput = new RichTextBox
		{
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			Background = Brushes.White,
			IsReadOnly = true,
			IsDocumentEnabled = true,
			//FontFamily = new FontFamily("Comic Sans MS"),
		};

		//Input textbox
		private static TextBox mInput = new TextBox
		{
			MaxLength = 250,
			MaxLines = 5,
			TextWrapping = TextWrapping.Wrap,
		};
		//Input button
		private static Button mInputButton = new Button
		{
			IsEnabled = false,
			Content = "Enter",
		};

		//Latency
		private static TextBox mLatency = new TextBox
		{
			IsReadOnly = true,
		};
		//Memory
		private static TextBox mMemory = new TextBox
		{
			IsReadOnly = true,
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
		};

		//Secondary grids
		private static Grid mInputLayout = new Grid();
		private static Grid mSysInfoLayout = new Grid();

		#region Startup
		//Create the bot window
		public BotWindow()
		{
			InitializeComponent();
		}

		//Create all the components
		private void InitializeComponent()
		{
			//Main layout
			mLayout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(350) });
			mLayout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(25) });
			mLayout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
			mLayout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(510) });
			//Input layout
			mInputLayout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
			mInputLayout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(460) });
			mInputLayout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(50) });
			//System info layout
			mSysInfoLayout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(170) });
			mSysInfoLayout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(170) });
			mSysInfoLayout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(170) });

			//Output textbox
			mLayout.Children.Add(mOutput);
			//If the ouput box is a rich text box, lower its margins
			if (mOutput is RichTextBox)
			{
				mOutput.Document.Blocks.FirstBlock.Margin = new Thickness(0);
			}

			//Input layout
			mLayout.Children.Add(mInputLayout);
			Grid.SetRow(mInputLayout, 2);
			//Input textbox
			mInputLayout.Children.Add(mInput);
			//Input button
			mInputLayout.Children.Add(mInputButton);
			Grid.SetColumn(mInputButton, 1);

			//System info layout
			mLayout.Children.Add(mSysInfoLayout);
			Grid.SetRow(mSysInfoLayout, 1);
			//Latency textbox
			mSysInfoLayout.Children.Add(mLatency);
			//Memory textbox
			mSysInfoLayout.Children.Add(mMemory);
			Grid.SetColumn(mMemory, 1);
			//Thread textbox
			mSysInfoLayout.Children.Add(mThreads);
			Grid.SetColumn(mThreads, 2);

			//Set this panel as the content for this window.
			Content = mLayout;

			//Actually run the application
			RunApplication();
		}

		//Do all the actions
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
			mMemory.MouseEnter += ModifyMemHoverInfo;
			mMemory.MouseLeave += ModifyMemHoverInfo;
		}
		#endregion

		#region Mutators
		//Add a hypderlink to the output box
		public static void AddHyperlink(string input)
		{
			//Make sure the input is a valid link
			if (!Actions.ValidateURL(input))
			{
				Actions.WriteLine(Actions.ERROR("Invalid URL."));
				return;
			}
			//Check if the paragraph is valid
			var para = (Paragraph)mOutput.Document.Blocks.LastBlock;
			if (para == null)
			{
				Actions.WriteLine(input);
				return;
			}
			//Create the hyperlink
			var hyperlink = new Hyperlink(new Run("Listed Guilds"))
			{
				NavigateUri = new Uri(input),
				IsEnabled = true,
			};
			//Format the paragraph
			para.Inlines.Add(new Run(DateTime.Now.ToString("HH:mm:ss") + ": "));
			//Add in the hyperlink
			para.Inlines.Add(hyperlink);
			//Add in a carriage return to have things look nicer
			para.Inlines.Add(new Run("\r"));
			//Add the paragraph to the ouput
			mOutput.Document.Blocks.Add(para);

			//Make it work when clicked
			hyperlink.RequestNavigate += (sender, link) =>
			{
				Process.Start(link.Uri.ToString());
				link.Handled = true;
			};
		}
		#endregion

		#region Event Actions
		//Accept input through the enter key
		private void AcceptInput(object sender, KeyEventArgs e)
		{
			if (String.IsNullOrWhiteSpace(mInput.Text))
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
					mInputButton.IsEnabled = true;
				}
			}
		}

		//Accept input through the enter button
		private void AcceptInput(object sender, RoutedEventArgs e)
		{
			GatherInput();
		}

		//Modify the memory tooltip
		private void ModifyMemHoverInfo(object sender, MouseEventArgs e)
		{
			if (mMemHoverInfo.IsOpen)
			{
				mMemHoverInfo.IsOpen = false;
			}
			else
			{
				mMemHoverInfo.IsOpen = true;
			}
		}
		#endregion

		//Gather the input and reset the input
		private void GatherInput()
		{
			//Get the current text
			var text = mInput.Text;
			//Write it out to the ghetto console
			Console.WriteLine(text);
			//Clear the input
			mInput.Text = "";
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
				mLatency.Text = String.Format("Latency: {0}ms", Variables.Client.Latency);
				mMemory.Text = String.Format("Memory: {0}mb", (Process.GetCurrentProcess().WorkingSet64 / 1000000.0).ToString("0.00"));
				mThreads.Text = String.Format("Threads: {0}", Process.GetCurrentProcess().Threads.Count);
			};
			//Make the timer update every so often
			timer.Interval = new TimeSpan(0, 0, 0, 0, 250);
			//Start the timer
			timer.Start();
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
			if (cmd.Equals("pause", StringComparison.OrdinalIgnoreCase))
			{
				UICommands.PAUSE(args);
			}
			else if (cmd.Equals("globalbotowner", StringComparison.OrdinalIgnoreCase) || cmd.Equals("glbo", StringComparison.OrdinalIgnoreCase))
			{
				Task.Run(async () =>
				{
					await UICommands.UIGlobalBotOwner(args);
				});
			}
			else if (cmd.Equals("globalsavepath", StringComparison.OrdinalIgnoreCase) || cmd.Equals("glsp", StringComparison.OrdinalIgnoreCase))
			{
				UICommands.UIGlobalSavePath(args);
			}
			else if (cmd.Equals("globalprefix", StringComparison.OrdinalIgnoreCase) || cmd.Equals("glp", StringComparison.OrdinalIgnoreCase))
			{
				Task.Run(async () =>
				{
					await UICommands.UIGlobalPrefix(args);
				});
			}
			else if (cmd.Equals("globalsettings", StringComparison.OrdinalIgnoreCase) || cmd.Equals("gls", StringComparison.OrdinalIgnoreCase))
			{
				UICommands.UIGlobalSettings(args);
			}
			else if (cmd.Equals("boticon", StringComparison.OrdinalIgnoreCase) || cmd.Equals("bi", StringComparison.OrdinalIgnoreCase))
			{
				Task.Run(async () =>
				{
					await UICommands.UIBotIcon(args);
				});
			}
			else if (cmd.Equals("botgame", StringComparison.OrdinalIgnoreCase) || cmd.Equals("bg", StringComparison.OrdinalIgnoreCase))
			{
				Task.Run(async () =>
				{
					await UICommands.UIBotGame(args);
				});
			}
			else if (cmd.Equals("botstream", StringComparison.OrdinalIgnoreCase) || cmd.Equals("bst", StringComparison.OrdinalIgnoreCase))
			{
				Task.Run(async () =>
				{
					await UICommands.UIBotStream(args);
				});
			}
			else if (cmd.Equals("disconnect", StringComparison.OrdinalIgnoreCase) || cmd.Equals("dc", StringComparison.OrdinalIgnoreCase) || cmd.Equals("runescapeservers", StringComparison.OrdinalIgnoreCase))
			{
				UICommands.UIDisconnect(args);
			}
			else if (cmd.Equals("restart", StringComparison.OrdinalIgnoreCase) || cmd.Equals("res", StringComparison.OrdinalIgnoreCase))
			{
				UICommands.UIRestart(args);
			}
			else if (cmd.Equals("listguilds", StringComparison.OrdinalIgnoreCase) || cmd.Equals("lgds", StringComparison.OrdinalIgnoreCase))
			{
				UICommands.UIListGuilds();
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
				Variables.STOP = true;
			}
			else if (input.Equals("off", StringComparison.OrdinalIgnoreCase))
			{
				Actions.WriteLine("Successfully unpaused the bot.");
				Variables.STOP = false;
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
		public static async Task UIGlobalBotOwner(string input)
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
				var user = await Actions.GetBotOwner(Variables.Client);
				if (user != null)
				{
					Actions.WriteLine(String.Format("The current bot owner is: `{0}#{1} ({2})`", user.Username, user.Discriminator, user.Id));
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
				var user = await Actions.GetBotOwner(Variables.Client);
				Actions.WriteLine(String.Format("There is already a bot owner: `{0}#{1} ({2})`.", user.Username, user.Discriminator, user.Id));
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
			var globalUser = Variables.Client.GetUser(ID);
			if (globalUser == null)
			{
				Actions.WriteLine("Unable to find any users with that ID.");
				return;
			}

			Properties.Settings.Default.BotOwner = globalUser.Id;
			Properties.Settings.Default.Save();
			Actions.WriteLine(String.Format("Successfully made `{0}#{1} ({2})` the new bot owner.", globalUser.Username, globalUser.Discriminator, globalUser.Id));
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
				description += String.Format("Prefix: {0}", String.IsNullOrWhiteSpace(Properties.Settings.Default.Prefix) ? "N/A" : Properties.Settings.Default.Prefix);
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
				Properties.Settings.Default.Reset();
				//Restart the bot
				try
				{
					//Restart the application
					System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
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
				await Variables.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Discord.Image());
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
					await Variables.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Discord.Image(imgStream));
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

			await Variables.Client.SetGameAsync(input, Variables.Client.CurrentUser.Game.Value.StreamUrl, Variables.Client.CurrentUser.Game.Value.StreamType);
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
			await Variables.Client.SetGameAsync(Variables.Client.CurrentUser.Game.Value.Name, input, streamType);
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
				Actions.WriteLine(Actions.ERROR("Name cannot be more than 32 characters.."));
				return;
			}
			else if (input.Length < Constants.NICKNAME_MIN_LENGTH)
			{
				Actions.WriteLine(Actions.ERROR("Name cannot be less than 2 characters.."));
				return;
			}

			//Change the bots name to it
			await Variables.Client.CurrentUser.ModifyAsync(x => x.Username = input);

			//Send a success message
			Actions.WriteLine(String.Format("Successfully changed my username to `{0}`.", input));
		}

		//Disconnect the bot
		public static void UIDisconnect(string input)
		{
			Environment.Exit(0);
		}

		//Restart the bot
		public static void UIRestart(string input)
		{
			try
			{
				//Restart the application
				System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
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
			var guildStrings = Variables.Client.Guilds.ToList().Select(x => String.Format("{0}. {1} Owner: {2}#{3} ({4})",
				count++.ToString("00"), Actions.FormatGuild(x), x.Owner.Username, x.Owner.Discriminator, x.Owner.Id));

			//Send it to have the hyperlink created and go to the output window
			BotWindow.AddHyperlink(Actions.UploadToHastebin(String.Join("\n", guildStrings)));
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
}
