namespace Advobot.Services;

/// <summary>
/// A service which can be started and stopped.k
/// </summary>
public interface IStartableService
{
	/// <summary>
	/// Start the service.
	/// </summary>
	public void Start();

	/// <summary>
	/// Stop the service.
	/// </summary>
	public void Stop();
}