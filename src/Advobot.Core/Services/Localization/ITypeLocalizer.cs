namespace Advobot.Services.Localization
{
	public interface ITypeLocalizer
	{
		void Add<T>(string value, bool overwrite = false);

		bool TryGet<T>(out string? output);
	}
}