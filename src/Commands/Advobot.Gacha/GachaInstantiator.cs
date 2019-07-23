using Advobot.CommandMarking;
using Advobot.Gacha.Database;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Advobot.Gacha
{
	public sealed class GachaInstantiation : ICommandAssemblyInstantiator
	{
		public Task Instantiate(IServiceCollection services)
		{
			services.AddSingleton<GachaDatabase>();
			return Task.CompletedTask;
		}
	}
}
