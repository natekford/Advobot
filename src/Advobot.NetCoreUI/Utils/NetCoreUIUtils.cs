using System;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Avalonia.Media;
using Newtonsoft.Json;

namespace Advobot.NetCoreUI.Utils
{
	/// <summary>
	/// Utilities for the .Net Core UI.
	/// </summary>
	public static class NetCoreUIUtils
	{
		public static string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

		/// <summary>
		/// Because <see cref="ReactiveUI.ReactiveCommand"/> implements this method explicitly.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="parameter"></param>
		public static void Execute(this ICommand command, object parameter)
			=> command.Execute(parameter);
		public static FileInfo GenerateFileName(this IBotDirectoryAccessor accessor, string fileName, string ext = "txt")
			=> accessor.GetBaseBotDirectoryFile($"{fileName}_{FormattingUtils.ToSaving()}.{ext}");
		public static SaveStatus Save(this FileInfo file, string text, Type deserializeType = null)
		{
			if (deserializeType != null)
			{
				try
				{
					var throwaway = JsonConvert.DeserializeObject(text, deserializeType);
				}
				catch (JsonReaderException jre)
				{
					jre.Write();
					return SaveStatus.DeserializationError;
				}
			}

			try
			{
				IOUtils.SafeWriteAllText(file, text);
				return SaveStatus.Success;
			}
			catch
			{
				return SaveStatus.Failure;
			}
		}
		public static (string Text, ISolidColorBrush Background) SaveAndGetResponse(this FileInfo file, string text, Type deserializationType = null)
			=> file.Save(text, deserializationType).GetSaveResponse(file);
		public static (string Text, ISolidColorBrush Background) GetSaveResponse(this SaveStatus response, FileInfo file)
		{
			switch (response)
			{
				case SaveStatus.Failure:
					return ($"Unable to correctly save {file}.", Brushes.Red);
				case SaveStatus.Success:
					return ($"Successfully saved {file}.", Brushes.Green);
				case SaveStatus.DeserializationError:
					return ($"Deserialization error occurred during saving {file}. This is caused by putting invalid values in Json.", Brushes.Red);
				default:
					throw new ArgumentException(nameof(response));
			}
		}
	}

	public enum SaveStatus
	{
		Failure,
		Success,
		DeserializationError,
	}
}