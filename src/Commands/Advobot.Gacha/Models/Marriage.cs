namespace Advobot.Gacha.Models
{
	public class Marriage
	{
		public ulong TimeMarried { get; set; }
		public Image Image { get; set; } = new Image();

		public User User { get; set; } = new User();
		public Character Character { get; set; } = new Character();

		public Marriage() { }
		public Marriage(User user, Character character)
		{
			User = user;
			Character = character;
		}
	}
}
