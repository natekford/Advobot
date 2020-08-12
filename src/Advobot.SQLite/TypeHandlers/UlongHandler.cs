using System.Data;

using static Dapper.SqlMapper;

namespace Advobot.SQLite.TypeHandlers
{
	public sealed class UlongHandler : TypeHandler<ulong>
	{
		public override ulong Parse(object value)
			=> ulong.Parse((string)value);

		public override void SetValue(IDbDataParameter parameter, ulong value)
		{
			parameter.DbType = DbType.String;
			parameter.Value = value.ToString();
		}
	}
}