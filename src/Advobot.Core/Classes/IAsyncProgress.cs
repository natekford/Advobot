using System.Threading.Tasks;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Defines a provider for asynchronous progress updates.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IAsyncProgress<in T>
	{
		/// <summary>
		/// Reports a progress update asynchronously.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		Task ReportAsync(T value);
	}
}