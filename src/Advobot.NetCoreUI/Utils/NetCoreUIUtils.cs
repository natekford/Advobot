using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ReactiveUI;
using Newtonsoft.Json;
using AdvorangesUtils;
using Advobot.Interfaces;
using Advobot.Utilities;

namespace Advobot.NetCoreUI.Utils
{
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
		{
			command.Execute(parameter);
		}
		public static bool Save(IBotDirectoryAccessor accessor, string fileName, string text, Type deserializeType = null)
		{
			var file = accessor.GetBaseBotDirectoryFile($"{fileName}_{FormattingUtils.ToSaving()}.txt");
			if (deserializeType != null)
			{
				try
				{
					var throwaway = JsonConvert.DeserializeObject(text, deserializeType);
				}
				catch (JsonReaderException jre)
				{
					jre.Write();
					return false;
				}
			}

			try
			{
				IOUtils.SafeWriteAllText(file, text);
				return true;
			}
			catch
			{
				return false;
			}
		}
		public static string GetSaveResponse(bool? success)
		{
			if (!(success is bool val))
			{
				return "File not found.";
			}
			return val ? "Successfully saved." : "Failed to save.";
		}
	}
}