namespace Advobot.Gacha.Models
{
	public class Wish
	{
		public ulong TimeWished { get; set; }
		public User User { get; set; } = new User();
		public Character Character { get; set; } = new Character();
	}
}
