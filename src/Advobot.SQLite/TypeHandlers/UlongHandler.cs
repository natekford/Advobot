using System.Data;

using static Dapper.SqlMapper;

namespace Advobot.SQLite.TypeHandlers;

/// <summary>
/// Type handler to deal with ulongs in SQLite.
/// </summary>
public sealed class UlongHandler : TypeHandler<ulong>
{
	/// <inheritdoc />
	public override ulong Parse(object value)
		=> ulong.Parse((string)value);

	/// <inheritdoc />
	public override void SetValue(IDbDataParameter parameter, ulong value)
	{
		parameter.DbType = DbType.String;
		parameter.Value = value.ToString();
	}
}