namespace Advobot.Services;

/// <summary>
/// A service which should do something after being initialized.
/// </summary>
public interface IConfigurableService
{
	/// <summary>
	/// Something to do after all initialized.
	/// </summary>
	public Task ConfigureAsync();
}