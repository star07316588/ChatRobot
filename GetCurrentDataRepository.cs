// Repository public class ReportRepository { private readonly IDbConnection _dbConnection;

public ReportRepository(IDbConnection dbConnection)
{
    _dbConnection = dbConnection;
}

public string GetCurrentDate(string datepart, int dateadd)
{
    string sql;

    if (dateadd != 0)
    {
        if (datepart == "y")
        {
            sql = $"SELECT TO_CHAR(ADD_MONTHS(SYSDATE, {12 * dateadd}), 'yyyy') AS year, TO_CHAR(ADD_MONTHS(SYSDATE, {12 * dateadd}), 'mm') AS month, TO_CHAR(ADD_MONTHS(SYSDATE, {12 * dateadd}), 'dd') AS days FROM dual";
        }
        else if (datepart == "m" || datepart == "ym-1")
        {
            sql = $"SELECT TO_CHAR(ADD_MONTHS(SYSDATE, {dateadd}), 'yyyy') AS year, TO_CHAR(ADD_MONTHS(SYSDATE, {dateadd}), 'mm') AS month, TO_CHAR(ADD_MONTHS(SYSDATE, {dateadd}), 'dd') AS days FROM dual";
        }
        else
        {
            sql = $"SELECT TO_CHAR(SYSDATE + {dateadd}, 'yyyy') AS year, TO_CHAR(SYSDATE + {dateadd}, 'mm') AS month, TO_CHAR(SYSDATE + {dateadd}, 'dd') AS days FROM dual";
        }
    }
    else
    {
        sql = "SELECT TO_CHAR(SYSDATE, 'yyyy') AS year, TO_CHAR(SYSDATE, 'mm') AS month, TO_CHAR(SYSDATE, 'dd') AS days FROM dual";
    }

    var result = _dbConnection.QueryFirstOrDefault(sql);

    if (result == null) return "";

    switch (datepart)
    {
        case "y":
            return result.YEAR;
        case "m":
            return result.MONTH;
        case "ym":
            return result.YEAR + result.MONTH;
        case "ym-1":
            return result.YEAR;
        default:
            return result.DAYS;
    }
}

}

