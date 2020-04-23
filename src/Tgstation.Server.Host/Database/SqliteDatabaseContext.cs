﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using Tgstation.Server.Host.Configuration;

namespace Tgstation.Server.Host.Database
{
	/// <summary>
	/// <see cref="DatabaseContext{TParentContext}"/> for MySQL
	/// </summary>
	sealed class SqliteDatabaseContext : DatabaseContext<SqliteDatabaseContext>
	{
		/// <summary>
		/// Construct a <see cref="MySqlDatabaseContext"/>
		/// </summary>
		/// <param name="dbContextOptions">The <see cref="DbContextOptions{TContext}"/> for the <see cref="DatabaseContext{TParentContext}"/></param>
		/// <param name="databaseConfiguration">The <see cref="IOptions{TOptions}"/> of <see cref="DatabaseConfiguration"/> for the <see cref="DatabaseContext{TParentContext}"/></param>
		/// <param name="databaseSeeder">The <see cref="IDatabaseSeeder"/> for the <see cref="DatabaseContext{TParentContext}"/></param>
		/// <param name="logger">The <see cref="ILogger"/> for the <see cref="DatabaseContext{TParentContext}"/></param>
		public SqliteDatabaseContext(DbContextOptions<SqliteDatabaseContext> dbContextOptions, IOptions<DatabaseConfiguration> databaseConfiguration, IDatabaseSeeder databaseSeeder, ILogger<SqliteDatabaseContext> logger) : base(dbContextOptions, databaseConfiguration, databaseSeeder, logger)
		{ }

		/// <inheritdoc />
		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			base.OnConfiguring(options);
			options.UseSqlite(DatabaseConfiguration.ConnectionString);
		}

		/// <inheritdoc />
		protected override void ValidateDatabaseType()
		{
			if (DatabaseType != DatabaseType.Sqlite)
				throw new InvalidOperationException("Invalid DatabaseType for SqliteDatabaseContext!");
		}
	}
}
