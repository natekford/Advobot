﻿using System.Windows.Input;

using Advobot.Settings;
using Advobot.Utilities;

using AdvorangesUtils;

using Avalonia.Media;

using Newtonsoft.Json;

namespace Advobot.UI.Utils
{
	public enum SaveStatus
	{
		Failure,
		Success,
		DeserializationError,
	}

	/// <summary>
	/// Utilities for the .Net Core UI.
	/// </summary>
	public static class NetCoreUIUtils
	{
		/// <summary>
		/// Because <see cref="ReactiveUI.ReactiveCommand"/> implements this method explicitly.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="parameter"></param>
		public static void Execute(this ICommand command, object parameter)
			=> command.Execute(parameter);

		public static FileInfo GenerateFileName(this IBotDirectoryAccessor accessor, string fileName, string ext = "txt")
			=> accessor.GetBaseBotDirectoryFile($"{fileName}_{AdvorangesUtils.FormattingUtils.ToSaving()}.{ext}");

		public static (string Text, ISolidColorBrush Background) GetSaveResponse(this SaveStatus response, FileInfo file) => response switch
		{
			SaveStatus.Failure => ($"Unable to correctly save {file}.", Brushes.Red),
			SaveStatus.Success => ($"Successfully saved {file}.", Brushes.Green),
			SaveStatus.DeserializationError => ($"Deserialization error occurred during saving {file}. This is caused by putting invalid values in Json.", Brushes.Red),
			_ => throw new ArgumentOutOfRangeException(nameof(response)),
		};

		public static SaveStatus Save(this FileInfo file, string text, Type? deserializationType = null)
		{
			if (deserializationType != null)
			{
				try
				{
					var throwaway = JsonConvert.DeserializeObject(text, deserializationType);
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

		public static (string Text, ISolidColorBrush Background) SaveAndGetResponse(this FileInfo file, string text, Type? deserializationType = null)
			=> file.Save(text, deserializationType).GetSaveResponse(file);
	}
}