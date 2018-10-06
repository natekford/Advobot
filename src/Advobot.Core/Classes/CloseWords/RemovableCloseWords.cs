using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Commands;

namespace Advobot.Classes.CloseWords
{
	/// <summary>
	/// Container of close words which is intended to be removed after the time has passed.
	/// </summary>
	public sealed class RemovableCloseWords : RemovableMessage
	{
		/// <summary>
		/// The gathered words.
		/// </summary>
		public List<CloseWord<string>> List { get; set; }
		/// <summary>
		/// The type of close words these are, e.g quote or help entries.
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="RemovableCloseWords"/>. Parameterless constructor is used for the database.
		/// </summary>
		public RemovableCloseWords() : base() { }
		/// <summary>
		/// Creates an instance of <see cref="RemovableCloseWords"/>.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="closeWords"></param>
		/// <param name="time"></param>
		/// <param name="messages"></param>
		/// <param name="context"></param>
		public RemovableCloseWords(string type, IEnumerable<CloseWord<string>> closeWords, ICommandContext context, IEnumerable<IMessage> messages, TimeSpan time = default)
			: base(context, messages, time)
		{
			List = closeWords.ToList();
			Type = type;
		}
	}
}