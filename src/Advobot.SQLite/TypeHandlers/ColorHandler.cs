using Discord;

using System.Data;

using static Dapper.SqlMapper;

namespace Advobot.SQLite.TypeHandlers;

/// <summary>
/// Type handler to deal with <see cref="Color"/> in SQLite.
/// </summary>
public sealed class ColorHandler : TypeHandler<Color>
{
	/// <inheritdoc />
	public override Color Parse(object value)
		=> new((uint)(long)value);

	/// <inheritdoc />
	public override void SetValue(IDbDataParameter parameter, Color value)
	{
		parameter.DbType = DbType.UInt32;
		parameter.Value = value.RawValue;
	}
}