namespace Advobot.Gacha.Models
{
	public class Alias
	{
		public string Name { get; set; } = "";
		public bool IsSpoiler { get; set; }

		public Character Character { get; set; } = new Character();
	}
}
