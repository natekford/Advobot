using System;

namespace Advobot.Classes.UsageGeneration
{
	/// <summary>
	/// Information about something to be used in <see cref="UsageGenerator"/>.
	/// </summary>
	internal class UsageDetails
	{
		public int Deepness { get; protected set; }
		public string Name { get; protected set; }

		public UsageDetails(int deepness, string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(nameof(name));
			}

			Deepness = deepness;
#pragma warning disable CS8620 // Nullability of reference types in argument doesn't match target type.
			Name = name.Length == 1 ? name.ToUpper() : name[0].ToString().ToUpper() + name[1..];
#pragma warning restore CS8620 // Nullability of reference types in argument doesn't match target type.
		}

		public override string ToString()
			=> Name;
	}
}