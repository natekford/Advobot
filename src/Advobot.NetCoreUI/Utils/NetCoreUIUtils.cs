using System;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
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
		public static SaveResponse Save(this FileInfo file, string text, Type deserializeType = null)
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
					return SaveResponse.DeserializationError;
				}
			}

			try
			{
				IOUtils.SafeWriteAllText(file, text);
				return SaveResponse.Success;
			}
			catch
			{
				return SaveResponse.Failure;
			}
		}
		//TODO: make this notify in the window instead of only in main 
		public static string SaveAndGetResponse(this FileInfo file, string text, Type deserializationType = null) => file.Save(text, deserializationType).GetSaveResponse(file);
		public static string GetSaveResponse(this SaveResponse response, FileInfo file)
		{
			switch (response)
			{
				case SaveResponse.Failure:
					return $"Unable to correctly save {file}.";
				case SaveResponse.Success:
					return $"Successfully saved {file}.";
				case SaveResponse.DeserializationError:
					return $"Deserialization error occurred during saving {file}. This is caused by putting invalid values in Json.";
				default:
					throw new ArgumentException(nameof(response));
			}
		}
	}

	public enum SaveResponse
	{
		Failure,
		Success,
		DeserializationError,
	}
}