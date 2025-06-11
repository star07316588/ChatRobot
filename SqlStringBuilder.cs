public static string BuildExecutableSql(string sql, DynamicParameters parameters)
{
    foreach (var paramName in parameters.ParameterNames)
    {
        var value = parameters.Get<dynamic>(paramName);
        string formattedValue = value is string || value is DateTime
            ? $"'{value}'"
            : value?.ToString() ?? "NULL";

        sql = sql.Replace($"@{paramName}", formattedValue);
    }
    return sql;
}