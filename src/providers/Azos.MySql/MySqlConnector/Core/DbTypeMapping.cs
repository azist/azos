/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Data;

namespace MySqlConnector.Core
{
	internal sealed class DbTypeMapping
	{
		public DbTypeMapping(Type clrType, DbType[] dbTypes, Func<object, object> convert = null)
		{
			ClrType = clrType;
			DbTypes = dbTypes;
			m_convert = convert;
		}

		public Type ClrType { get; }
		public DbType[] DbTypes { get; }

		public object DoConversion(object obj)
		{
			if (obj.GetType() == ClrType)
				return obj;
			return m_convert == null ? Convert.ChangeType(obj, ClrType) : m_convert(obj);
		}

		readonly Func<object, object> m_convert;
	}
}
