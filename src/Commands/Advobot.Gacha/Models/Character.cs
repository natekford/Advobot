using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Relationships;
using Advobot.Gacha.Utils;
using System;
using System.Collections.Generic;

namespace Advobot.Gacha.Models
{
	public class Character : IReadOnlyCharacter
	{
		public int SourceId { get; private set; }
		public Source Source
		{
			get => _Source ?? throw new InvalidOperationException($"Character.Source is not set.");
			set
			{
				SourceId = value.SourceId;
				_Source = value;
			}
		}
		private Source? _Source;

		public int CharacterId { get; set; }
		public string Name { get; set; }
		public string GenderIcon { get; set; }
		public Gender Gender { get; set; }
		public RollType RollType { get; set; }
		public string? FlavorText { get; set; }
		public bool IsFakeCharacter { get; set; }
		public IList<Image> Images { get; set; } = new List<Image>();
		public IList<Alias> Aliases { get; set; } = new List<Alias>();

		public long TimeCreated { get; set; } = TimeUtils.Now();

		IReadOnlySource ISourceChild.Source => Source;
		IReadOnlyList<IReadOnlyImage> IReadOnlyCharacter.Images => (IReadOnlyList<IReadOnlyImage>)Images;
		IReadOnlyList<IReadOnlyAlias> IReadOnlyCharacter.Aliases => (IReadOnlyList<IReadOnlyAlias>)Aliases;
	}
}
