﻿using System;
using System.Data;

namespace DbSharper2.Schema.Provider
{
	public abstract class SchemaProviderBase
	{
		#region Methods

		/// <summary>
		/// Build sql name of a parameter.
		/// </summary>
		/// <param name="parameterName">Parameter name.</param>
		/// <returns>Sql name of parameter</returns>
		public abstract string BuildParameterSqlName(string parameterName);

		public abstract Type GetDatabaseType();

		public abstract DbType GetDbType(string dbTypeString);

		/// <summary>
		/// Get name of the parameter.
		/// </summary>
		/// <param name="parameter">Parameter.</param>
		/// <returns>Name</returns>
		public abstract string GetParameterName(string parameter);

		public abstract Database.Database GetSchema(string connectionString);

		#endregion Methods
	}
}