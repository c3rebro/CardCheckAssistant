using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using CardCheckAssistant.Models;
using Log4CSharp;

namespace CardCheckAssistant.Services;

public class SQLDBService : IDisposable
{
    public bool IsConnected { get; private set; }

    private readonly string? serverName;
    private readonly string dbName;
    private readonly string? usr;
    private readonly string? pwd;

#if DEBUG
    private const string OMNIDBNAME = "OT_CardCheck_Test";
    private const string OMNITABLENAME = "T_CardCheck";
    private readonly string[] OMNITABLECOLUMNS = { 
        "CC_ID VARCHAR(25)",
        "CC_JobNumber VARCHAR(25)",
        "CC_CardNumber VARCHAR(25)",
        "CC_CreationDate VARCHAR(25)",
        "CC_ChangedDate VARCHAR(25)",
        "CC_CustomerName VARCHAR(50)",
        "CC_Status VARCHAR(25)", 
        "CC_Report BLOB",
        "CC_EditorName VARCHAR(25)",
        "CC_EditorComment TEXT" };

    private const string RFIDTABLENAME = "T_RFIDGEAR";
    private readonly string[] RFIDTABLECOLUMNS = {
        "RG_ID VARCHAR(25)",
        "RG_ChipType VARCHAR(25)",
        "RG_ATS VARCHAR(25)",
        "RG_SAK VARCHAR(25)",
        "RG_L4Version VARCHAR(25)"};

#else
    private const string OMNITABLENAME = "T_CardCheck";
    private const string OMNIDBNAME = "OT_CardCheck_Test";
    private readonly string[] OMNITABLECOLUMNS = { "CC-ID", "CC-JobNumber", "CC-CardNumber", "CC-CreationDate", "CC-ChangedDate", "CC-CustomerName", "CC-Status", "CC-Report", "CC-EditorName", "CC-EditorComment" };
    private const string RFIDTABLENAME = "T_RFIDGEAR";
    private readonly string[] RFIDTABLECOLUMNS = {
        "RG_ID VARCHAR(25)",
        "RG_ChipType VARCHAR(25)",
        "RG_ATS VARCHAR(25)",
        "RG_SAK VARCHAR(25)",
        "RG_L4Version VARCHAR(25)"};
#endif
   
    public SQLDBService(string _dbName)
    {
        dbName = _dbName;
    }

    public SQLDBService(string _server, string _dbName, string _userID, string _pwd)
    {
        serverName = _server;
        dbName = _dbName;
        usr = _userID;
        pwd = _pwd;
    }

    public async Task CreateSqlLiteDBandTableAsync()
    {
        try
        {
            using (SQLiteConnection sqlLiteConnection = new SQLiteConnection("Data Source=" +
                dbName + ".db; Version = 3; New = True; Compress = True; "))
            {
                // Open the connection:
                await sqlLiteConnection.OpenAsync();

                if (!TableExists(sqlLiteConnection, OMNITABLENAME))
                {
                    await CreateTableAsync(sqlLiteConnection, OMNITABLENAME, OMNITABLECOLUMNS);
                }

                if (!TableExists(sqlLiteConnection, RFIDTABLENAME))
                {
                    await CreateTableAsync(sqlLiteConnection, RFIDTABLENAME, RFIDTABLECOLUMNS);
                }
            }
        }
        catch (Exception ex)
        {
            LogWriter.CreateLogEntry(ex, Assembly.GetExecutingAssembly().GetName().Name);
        }
    }

    public async Task CreateSQLDBandTableAsync()
    {
        try
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = serverName;
            //builder.UserID = userID;
            //builder.Password = pwd;
            builder.IntegratedSecurity = true;
            //builder.Encrypt = true;
            builder.InitialCatalog = dbName;
            builder.ApplicationName = serverName.Split('\\')[1];

            using (SqlConnection sqlConnection = new SqlConnection(builder.ConnectionString))
            {
                await sqlConnection.OpenAsync();

                if (!TableExists(sqlConnection, OMNIDBNAME, OMNITABLENAME))
                {
                    //await CreateTableAsync(sqlConnection, OMNITABLENAME, OMNITABLECOLUMNS);
                }

                if (!TableExists(sqlConnection, OMNIDBNAME, RFIDTABLENAME))
                {
                    //await CreateTableAsync(sqlConnection, RFIDTABLENAME, RFIDTABLECOLUMNS);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    public async Task<ObservableCollection<CardCheckProcess>> GetCardChecksFromMSSQLAsync()
    {
        try
        {
            CardCheckProcess cardCheckProcess;
            List<CardCheckProcess> cardChecks = new List<CardCheckProcess>();

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = serverName;
            builder.IntegratedSecurity = true;
            builder.InitialCatalog = dbName;
            builder.ConnectRetryInterval = 10;
            builder.ConnectRetryCount = 2;
            builder.ConnectTimeout = 30;
            builder.PacketSize = 512;
            builder.ApplicationName = serverName.Split('\\')[1];

            using (SqlConnection sqlConnection = new SqlConnection(builder.ConnectionString))
            {
                if (sqlConnection != null)
                {
                    if (sqlConnection.State == System.Data.ConnectionState.Closed)
                    {
                        await sqlConnection.OpenAsync();
                        IsConnected = true;
                    }

                    using (SqlCommand sql_cmd = sqlConnection.CreateCommand())
                    {
                        sql_cmd.CommandText = "SELECT * FROM " + OMNITABLENAME;

                        using (SqlDataReader sql_datareader = await sql_cmd.ExecuteReaderAsync())
                        {
                            while (await sql_datareader.ReadAsync())
                            {
                                try
                                {
                                    cardCheckProcess = new CardCheckProcess
                                    {
                                        ID = await sql_datareader.IsDBNullAsync(2) ? null : sql_datareader.GetString(0),
                                        JobNr = await sql_datareader.IsDBNullAsync(1) ? null : sql_datareader.GetString(1),
                                        ChipNumber = await sql_datareader.IsDBNullAsync(2) ? null : sql_datareader.GetString(2),
                                        Date = await sql_datareader.IsDBNullAsync(3) ? null : sql_datareader.GetString(3),

                                        CName = await sql_datareader.IsDBNullAsync(5) ? null : sql_datareader.GetString(5),
                                        EditorName = await sql_datareader.IsDBNullAsync(6) ? null : sql_datareader.GetString(6),
                                        Status = (OrderStatus)Enum.Parse(typeof(OrderStatus), await sql_datareader.IsDBNullAsync(8) ? null : sql_datareader.GetString(8))
                                    };
                                    cardChecks.Add(cardCheckProcess);
                                }
                                catch(Exception ex)
                                {
                                    LogWriter.CreateLogEntry(ex, Assembly.GetExecutingAssembly().GetName().Name);
                                }
                            }
                        }
                    }
                    return new ObservableCollection<CardCheckProcess>(cardChecks);
                }
            }
        }
        catch (Exception ex)
        {
            LogWriter.CreateLogEntry(ex, Assembly.GetExecutingAssembly().GetName().Name);
            IsConnected = false;
        }

        return null;
    }

    public async Task<ObservableCollection<CardCheckProcess>> GetCardChecksFromSQLLiteAsync()
    {
        try
        {
            CardCheckProcess cardCheckProcess;
            List<CardCheckProcess> cardChecks = new List<CardCheckProcess>();

            using (SQLiteConnection sqlLiteConnection = new SQLiteConnection("Data Source=" +
                    dbName + ".db; Version = 3; Pooling = False; New = True; Compress = True; Timeout = 5"))
            {
                if (sqlLiteConnection != null)
                {
                    if (sqlLiteConnection.State == System.Data.ConnectionState.Closed)
                    {
                        await sqlLiteConnection.OpenAsync();
                    }

                    using (SQLiteCommand sqlite_cmd = sqlLiteConnection.CreateCommand())
                    {
                        sqlite_cmd.CommandText = "SELECT * FROM " + OMNITABLENAME;

                        using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                        {
                            while (await sqlite_datareader.ReadAsync())
                            {
                                cardCheckProcess = new CardCheckProcess
                                {
                                    
                                    ID = sqlite_datareader.GetString(0),
                                    ChipNumber = sqlite_datareader.GetString(2),
                                    CName = sqlite_datareader.GetString(5),
                                    JobNr = sqlite_datareader.GetString(1),
                                    EditorName = sqlite_datareader.GetString(8),
                                    //NumberOfChipsToCheck = sqlite_datareader.GetInt32(3),
                                    Status = (OrderStatus)Enum.Parse(typeof(OrderStatus), sqlite_datareader.GetString(6)),
                                    Date = sqlite_datareader.GetString(3),
                                };

                                cardChecks.Add(cardCheckProcess);
                            }
                            return new ObservableCollection<CardCheckProcess>(cardChecks);
                        }
                    };
                }
            };    
        }
        catch (Exception ex)
        {
            LogWriter.CreateLogEntry(ex, Assembly.GetExecutingAssembly().GetName().Name);
        }

        return null;
    }

    private async Task CreateTableAsync(SQLiteConnection conn, string tableName, string[] columns)
    {
        try
        {
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }

            var dbCmd = conn.CreateCommand();
            dbCmd.CommandText = "CREATE TABLE " + tableName + " (" + string.Join(", ", columns) + ")";
            await dbCmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e, Assembly.GetExecutingAssembly().GetName().Name);
        }
    }

    private async Task CreateTableAsync(SqlConnection conn, string dbName, string tableName)
    {
        try
        {
            var dbCmd = conn.CreateCommand();
            dbCmd.CommandText = "USE " + dbName;
            await dbCmd.ExecuteNonQueryAsync();

            conn.Close();
        }
        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e, Assembly.GetExecutingAssembly().GetName().Name);
        }
    }

    private void InsertData(SQLiteConnection conn)
    {
        SQLiteCommand sqlite_cmd;
        sqlite_cmd = conn.CreateCommand();
        sqlite_cmd.CommandText = "INSERT INTO CheckData" +
           " (ReportLanguage, CustomerName, JobNumber, NumberOfChips, Status, Date) VALUES('German', 'Bosch', 1234, 1, 'Neu', '10.01.2022'); ";
         sqlite_cmd.ExecuteNonQuery();
    }

    private void ReadData(SQLiteConnection conn)
    {
        SQLiteDataReader sqlite_datareader;
        SQLiteCommand sqlite_cmd;
        sqlite_cmd = conn.CreateCommand();
        sqlite_cmd.CommandText = "SELECT * FROM CheckData";

        sqlite_datareader = sqlite_cmd.ExecuteReader();
        while (sqlite_datareader.Read())
        {
            var myreader = sqlite_datareader.GetString(0);
            Console.WriteLine(myreader);
        }
    }

    private bool TableExists(SQLiteConnection conn, string tableName)
    {
        try
        {
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            var sql =
            "SELECT name FROM sqlite_master WHERE type='table' AND name='" + tableName + "';";
            if (conn.State == System.Data.ConnectionState.Open)
            {
                SQLiteCommand command = new SQLiteCommand(sql, conn);
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    return true;
                }
                return false;
            }
            else
            {
                throw new System.ArgumentException("Data.ConnectionState must be open");
            }
        }
        catch(Exception ex)
        {
            LogWriter.CreateLogEntry(ex, Assembly.GetExecutingAssembly().GetName().Name);
            return false;
        }

    }

    private bool TableExists(SqlConnection conn, string dbName, string tableName)
    {
        var sql =
        "SELECT name FROM " + dbName + " WHERE type='table' AND name='" + tableName + "';";
        if (conn.State == System.Data.ConnectionState.Open)
        {
            SqlCommand command = new SqlCommand(sql, conn);
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                return true;
            }
            return false;
        }
        else
        {
            throw new System.ArgumentException("Data.ConnectionState must be open");
        }
    }

    protected void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        _disposed = false;
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    private bool _disposed;
}