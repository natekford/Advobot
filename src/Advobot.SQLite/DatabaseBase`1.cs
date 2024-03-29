﻿using Advobot.SQLite.TypeHandlers;

using AdvorangesUtils;

using Dapper;

using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Advobot.SQLite;

/// <summary>
/// Base class for a SQL database.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class DatabaseBase<T> where T : DbConnection, new()
{
	/// <summary>
	/// Starts the database.
	/// </summary>
	protected IConnectionString Connection { get; }

	static DatabaseBase()
	{
		// SQLite can't handle ulongs, it stores them as longs and allows overflow
		// This causes issues when comparing values so we can either
		// 1) Store them as longs and account for overflow
		// 2) Store them as strings
		// Storing them as strings uses more space in the db, but
		// it does make it more readable if looking through the database manually
		SqlMapper.AddTypeHandler(new UlongHandler());
		// Simply removing the type map causes issues, we need to overwrite it with string
		SqlMapper.AddTypeMap(typeof(ulong), DbType.String);
		SqlMapper.AddTypeMap(typeof(ulong?), DbType.String);

		ConsoleUtils.DebugWrite("Added in the ulong handler for SQLite.", nameof(DatabaseBase<T>));
	}

	/// <summary>
	/// Creates an instance of <see cref="DatabaseBase{T}"/>.
	/// </summary>
	/// <param name="conn"></param>
	protected DatabaseBase(IConnectionString conn)
	{
		Connection = conn;
		ConsoleUtils.DebugWrite($"Created database with the connection \"{conn.ConnectionString}\".", nameof(DatabaseBase<T>));
	}

	/// <summary>
	/// Executes a query to modify many values.
	/// </summary>
	/// <typeparam name="TParam"></typeparam>
	/// <param name="sql"></param>
	/// <param name="params"></param>
	/// <returns></returns>
	protected async Task<int> BulkModifyAsync<TParam>(string sql, IEnumerable<TParam> @params)
	{
		//Use a transaction to make bulk modifying way faster in SQLite
		using var connection = await GetConnectionAsync().CAF();
		await using var transaction = await connection.BeginTransactionAsync().CAF();

		var affectedRowCount = await connection.ExecuteAsync(sql, @params, transaction).CAF();
		transaction.Commit();
		return affectedRowCount;
	}

	/// <summary>
	/// Gets a database connection.
	/// </summary>
	/// <returns></returns>
	protected Task<T> GetConnectionAsync()
		=> Connection.GetConnectionAsync<T>();

	/// <summary>
	/// Executes a query to retrieve multiple values.
	/// </summary>
	/// <typeparam name="TRet"></typeparam>
	/// <param name="sql"></param>
	/// <param name="param"></param>
	/// <returns></returns>
	protected async Task<TRet[]> GetManyAsync<TRet>(string sql, object? param)
	{
		using var connection = await GetConnectionAsync().CAF();
		var result = await connection.QueryAsync<TRet>(sql, param).CAF();
		return result.ToArray();
	}

	/// <summary>
	/// Executes a query to retrieve one value.
	/// </summary>
	/// <typeparam name="TRet"></typeparam>
	/// <param name="sql"></param>
	/// <param name="param"></param>
	/// <returns></returns>
	protected async Task<TRet?> GetOneAsync<TRet>(string sql, object param)
	{
		using var connection = await GetConnectionAsync().CAF();
		return await connection.QuerySingleOrDefaultAsync<TRet>(sql, param).CAF();
	}

	/// <summary>
	/// Executes a query to modify one value.
	/// </summary>
	/// <typeparam name="TParam"></typeparam>
	/// <param name="sql"></param>
	/// <param name="param"></param>
	/// <returns></returns>
	protected async Task<int> ModifyAsync<TParam>(string sql, TParam param)
	{
		using var connection = await GetConnectionAsync().CAF();
		return await connection.ExecuteAsync(sql, param).CAF();
	}
}