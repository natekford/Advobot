using System.Threading.Tasks;

using AdvorangesUtils;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Advobot.Classes
{
	/// <summary>
	/// Utilities for <see cref="AmbiguousUser"/>.
	/// </summary>
	public static class AmbiguousUserUtils
	{
		/// <summary>
		/// Converts the <see cref="IUser"/> to an <see cref="AmbiguousUser"/>.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static AmbiguousUser AsAmbiguous(this IUser user)
			=> new AmbiguousUser(user);
	}

	/// <summary>
	/// Used when it's unknown if the user is in the cache.
	/// </summary>
	public sealed class AmbiguousUser
	{
		private IUser? _User;

		/// <summary>
		/// The id of the user.
		/// </summary>
		public ulong Id { get; }

		/// <summary>
		/// Creates an instance of <see cref="AmbiguousUser"/> which will retrieve the user once asynchronously then return synchronously.
		/// </summary>
		/// <param name="id"></param>
		public AmbiguousUser(ulong id)
		{
			Id = id;
		}

		/// <summary>
		/// Creates an instance of <see cref="AmbiguousUser"/> which will return <paramref name="user"/> synchronously.
		/// </summary>
		/// <param name="user"></param>
		public AmbiguousUser(IUser user)
		{
			_User = user;
			Id = _User.Id;
		}

		/// <summary>
		/// Creates an instance of <see cref="AmbiguousUser"/> via the <see cref="IUser"/> constructor.
		/// </summary>
		/// <param name="user"></param>
		public static implicit operator AmbiguousUser(SocketUser user)
			=> new AmbiguousUser(user);

		/// <summary>
		/// Creates an instance of <see cref="AmbiguousUser"/> via the <see cref="IUser"/> constructor.
		/// </summary>
		/// <param name="user"></param>
		public static implicit operator AmbiguousUser(RestUser user)
			=> new AmbiguousUser(user);

		/// <summary>
		/// Creates an instance of <see cref="AmbiguousUser"/> via the <see cref="ulong"/> constructor.
		/// </summary>
		/// <param name="id"></param>
		public static implicit operator AmbiguousUser(ulong id)
			=> new AmbiguousUser(id);

		/// <summary>
		/// Retrieves the user and caches the result.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public ValueTask<IUser> GetAsync(BaseSocketClient client)
		{
			if (_User != null)
			{
				return new ValueTask<IUser>(_User);
			}
			else if (client.GetUser(Id) is IUser cached)
			{
				return new ValueTask<IUser>(cached);
			}

			static async Task<IUser> GetAsync(DiscordSocketRestClient rest, AmbiguousUser @this)
			{
				var user = await rest.GetUserAsync(@this.Id).CAF();
				@this._User = user;
				return user;
			}

			return new ValueTask<IUser>(GetAsync(client.Rest, this));
		}

		/// <summary>
		/// Retrieves the guild user and caches the result.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public ValueTask<IGuildUser> GetAsync(IGuild guild)
		{
			if (_User is IGuildUser temp)
			{
				return new ValueTask<IGuildUser>(temp);
			}

			static async Task<IGuildUser> GetAsync(IGuild guild, AmbiguousUser @this)
			{
				var user = await guild.GetUserAsync(@this.Id).CAF();
				@this._User = user;
				return user;
			}

			return new ValueTask<IGuildUser>(GetAsync(guild, this));
		}
	}
}