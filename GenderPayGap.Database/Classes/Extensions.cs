using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database.Classes
{
    public static class Extensions
    {

        internal static List<T> SqlQuery<T>(this IDbConnection connection, string query)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            //context.Database.OpenConnection();
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                using (IDataReader result = command.ExecuteReader())
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    var list = new List<T>();
                    T obj = default;
                    Type type = typeof(T);

                    while (result.Read())
                    {
                        if (type.IsSimpleType())
                        {
                            obj = (T) result.GetValue(0);
                        }
                        else
                        {
                            obj = Activator.CreateInstance<T>();

                            foreach (PropertyInfo prop in obj.GetType().GetProperties())
                            {
                                if (!Equals(result[prop.Name], DBNull.Value))
                                {
                                    prop.SetValue(obj, result[prop.Name], null);
                                }
                            }

                            foreach (FieldInfo field in obj.GetType().GetFields())
                            {
                                if (!Equals(result[field.Name], DBNull.Value))
                                {
                                    field.SetValue(obj, result[field.Name]);
                                }
                            }
                        }

                        list.Add(obj);
                    }

                    sw.Stop();
                    CustomLogger.Information($"Executed ({sw.ElapsedMilliseconds}ms)");
                    CustomLogger.Information($"{query}");

                    return list;
                }
            }
        }


        internal static int TableCount(this IDbConnection connection, string tableName = null, string schemaName = "dbo")
        {
            var query = "SELECT Count(*) FROM sys.tables";
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                query += $" t WHERE t.name = '{tableName}'";
                if (!string.IsNullOrWhiteSpace(schemaName))
                {
                    query =
                        $"SELECT Count(*) FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = '{schemaName}' AND t.name = '{tableName}'";
                }
            }

            int count = connection.SqlQuery<int>(query).FirstOrDefault().ToInt32();
            return count;
        }

        internal static int ViewCount(this IDbConnection connection, string viewName = null, string schemaName = "dbo")
        {
            var query = "SELECT Count(*) FROM sys.views";
            if (!string.IsNullOrWhiteSpace(viewName))
            {
                query += $" v WHERE v.name = '{viewName}'";
                if (!string.IsNullOrWhiteSpace(schemaName))
                {
                    query =
                        $"SELECT Count(*) FROM sys.views t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = '{schemaName}' AND t.name = '{viewName}'";
                }
            }

            int count = connection.SqlQuery<int>(query).FirstOrDefault().ToInt32();
            return count;
        }

    }
}
