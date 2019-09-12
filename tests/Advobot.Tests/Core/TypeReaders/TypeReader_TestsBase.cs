using System;
using System.Threading.Tasks;

using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Utilities;

using Discord.Commands;

namespace Advobot.Tests.Core.TypeReaders
{
	public abstract class TypeReader_TestsBase<T>
		where T : TypeReader, new()
	{
		protected FakeCommandContext Context { get; set; } = FakeUtils.CreateContext();
		protected T Instance { get; } = new T();
		protected IServiceProvider? Services { get; set; }

		protected Task<TypeReaderResult> ReadAsync(string input)
			=> Instance.ReadAsync(Context, input, Services);
	}
}