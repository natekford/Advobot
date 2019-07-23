using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Advobot.CommandMarking
{
	/// <summary>
	/// Specifies how to instantiate the command assembly.
	/// </summary>
	public interface ICommandAssemblyInstantiator
	{
		/// <summary>
		/// Does some start up work when the assembly in created.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		Task Instantiate(IServiceCollection services);
	}
}