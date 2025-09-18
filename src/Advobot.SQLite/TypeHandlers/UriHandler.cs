using System.Data;

using static Dapper.SqlMapper;

namespace Advobot.SQLite.TypeHandlers;

/// <summary>
/// Type handler to deal with <see cref="Uri"/> in SQLite.
/// </summary>
public sealed class UriHandler : TypeHandler<Uri>
{
	/// <inheritdoc />
	public override Uri? Parse(object value)
		=> value is string s ? new(s) : null;

	/// <inheritdoc />
	public override void SetValue(IDbDataParameter parameter, Uri? value)
	{
		parameter.DbType = DbType.String;
		parameter.Value = value?.ToString();
	}
}