using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.TypeReaders;
using System;
using System.Collections.Generic;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Allows a user to make an embed through the use of <see cref="CustomArguments{T}"/>.
	/// </summary>
	public class CustomEmbed
	{
		public const string FIELD_NAME = "FieldName";
		public const string FIELD_TEXT = "FieldText";
		public const string FIELD_INLINE = "FieldInline";
		public const string SPLIT_CHAR = "^";
		private static char _SplitChar = SPLIT_CHAR[0];
		public const string FORMAT = FIELD_NAME + ":Name" + SPLIT_CHAR + FIELD_TEXT + ":Text" + SPLIT_CHAR + FIELD_INLINE + ":True|False";

		public EmbedWrapper Embed { get; }

		[CustomArgumentConstructor]
		public CustomEmbed(
			[CustomArgument] string title,
			[CustomArgument] string description,
			[CustomArgument] string imageUrl,
			[CustomArgument] string url,
			[CustomArgument] string thumbUrl,
			[CustomArgument] string color,
			[CustomArgument] string authorName,
			[CustomArgument] string authorIconUrl,
			[CustomArgument] string authorUrl,
			[CustomArgument] string footer,
			[CustomArgument] string footerIconUrl,
			[CustomArgument(25)] params string[] fieldInfo)
		{
			this.Embed = new EmbedWrapper(title, description, ColorTypeReader.GetColor(color), imageUrl, url, thumbUrl)
				.AddAuthor(authorName, authorIconUrl, authorUrl)
				.AddFooter(footer, footerIconUrl);

			//Fields are done is a very gross way
			foreach (var f in fieldInfo)
			{
				//Split at max three since there are three parts to each field. Name, text, and inline.
				var split = f.Split(new[] { _SplitChar }, 3);
				if (split.Length < 2)
				{
					continue;
				}

				//Create a dict to store the values
				var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
				{
					{ FIELD_NAME, null },
					{ FIELD_TEXT, null },
					{ FIELD_INLINE, null },
				};
				//Get the values by the standard split by colon
				foreach (var arg in split)
				{
					var splitArg = arg.Split(new[] { ':' }, 2);
					if (splitArg.Length == 2 && dict.ContainsKey(splitArg[0]))
					{
						dict[splitArg[0]] = splitArg[1];
					}
				}

				//Fields cannot be set if the name or text is null
				if (String.IsNullOrWhiteSpace(dict[FIELD_NAME]) || String.IsNullOrWhiteSpace(dict[FIELD_TEXT]))
				{
					continue;
				}

				//Finally try to parse if the inline is a bool or not
				bool.TryParse(dict[FIELD_INLINE], out bool inline);
				this.Embed.AddField(dict[FIELD_NAME], dict[FIELD_TEXT], inline);
			}
		}
	}
}
