using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using CardCheckAssistant.Models;
using Elatec.NET;
using Log4CSharp;

namespace CardCheckAssistant.Services;

public class SQLDBService : IDisposable
{
    public bool IsConnected { get; private set; }
    public List<CardCheckProcess> CardChecks {get; private set;}

    private static readonly object syncRoot = new object();
    private static SQLDBService instance;

    private readonly string serverName = string.Empty;
    private readonly string dbName;
    private readonly string usr;
    private readonly string pwd;

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
        "CC_ReportBase64 VARCHAR(MAX)",
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

    public static SQLDBService Instance
    {
        get
        {
            lock (SQLDBService.syncRoot)
            {
                if (instance == null)
                {
                    instance = new SQLDBService(OMNIDBNAME);
                    return instance;

                }
                else
                {
                    return instance;
                }
            }
        }
    }

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

        instance = this;
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
            LogWriter.CreateLogEntry(ex);
        }
    }

    public async Task CreateSQLDBandTableAsync()
    {
        try
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = serverName;
            builder.IntegratedSecurity = true;
            builder.InitialCatalog = dbName;
            builder.ApplicationName = serverName.Split('\\')[1];

            using SqlConnection sqlConnection = new SqlConnection(builder.ConnectionString);

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
            CardChecks = new List<CardCheckProcess>();

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = serverName;
            builder.IntegratedSecurity = true;
            builder.InitialCatalog = dbName;
            builder.ConnectRetryInterval = 10;
            builder.ConnectRetryCount = 2;
            builder.ConnectTimeout = 30;
            builder.ApplicationName = serverName.Split('\\')[1];

            using SqlConnection sqlConnection = new SqlConnection(builder.ConnectionString);

            if (sqlConnection != null)
            {
                if (sqlConnection.State == System.Data.ConnectionState.Closed)
                {
                    await sqlConnection.OpenAsync();
                    IsConnected = true;
                }

                using (SqlCommand sql_cmd = sqlConnection.CreateCommand())
                {
                    sql_cmd.CommandText = "SELECT [CC-ID], [CC-JobNumber], [CC-CardNumber], [CC-CreationDate], [CC-Customername], [CC-EditorName], [CC-Status], [CC-DealerName], [CC-SalesName], [CC-ChangedDate] FROM " + OMNITABLENAME;

                    using SqlDataReader sql_datareader = await sql_cmd.ExecuteReaderAsync();

                    while (await sql_datareader.ReadAsync())
                    {
                        try
                        {
                            cardCheckProcess = new CardCheckProcess
                            {
                                ID = await sql_datareader.IsDBNullAsync(2) ? "" : sql_datareader.GetString(0),
                                JobNr = await sql_datareader.IsDBNullAsync(1) ? "" : sql_datareader.GetString(1),
                                ChipNumber = await sql_datareader.IsDBNullAsync(2) ? "" : sql_datareader.GetString(2),
                                DateCreated = await sql_datareader.IsDBNullAsync(3) ? "" : sql_datareader.GetString(3),
                                DealerName = await sql_datareader.IsDBNullAsync(7) ? "" : sql_datareader.GetString(7),
                                CName = await sql_datareader.IsDBNullAsync(5) ? "" : sql_datareader.GetString(4),
                                EditorName = await sql_datareader.IsDBNullAsync(6) ? "" : sql_datareader.GetString(5),
                                SalesName = await sql_datareader.IsDBNullAsync(8) ? "" : sql_datareader.GetString(8),
                                DateModified = await sql_datareader.IsDBNullAsync(9) ? "" : sql_datareader.GetString(9),
                                Status = await sql_datareader.IsDBNullAsync(6) ? "NA" : sql_datareader.GetString(6)

                            };
                            CardChecks.Add(cardCheckProcess);
                            CardChecks = CardChecks.OrderByDescending(x => x.DateCreated).ToList();
                        }
                        catch (Exception ex)
                        {
                            LogWriter.CreateLogEntry(ex);
                        }
                    }
                }
                return new ObservableCollection<CardCheckProcess>(CardChecks);
            }
        }
        catch (Exception ex)
        {
            LogWriter.CreateLogEntry(ex);
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

            using SQLiteConnection sqlLiteConnection = new SQLiteConnection("Data Source=" +
                    dbName + ".db; Version = 3; Pooling = False; New = True; Compress = True; Timeout = 5");

            if (sqlLiteConnection != null)
            {
                if (sqlLiteConnection.State == System.Data.ConnectionState.Closed)
                {
                    await sqlLiteConnection.OpenAsync();
                }

                using SQLiteCommand sqlite_cmd = sqlLiteConnection.CreateCommand();

                sqlite_cmd.CommandText = "SELECT * FROM " + OMNITABLENAME;

                using SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();

                while (await sqlite_datareader.ReadAsync())
                {
                    cardCheckProcess = new CardCheckProcess
                    {

                        ID = sqlite_datareader.GetString(0),
                        ChipNumber = sqlite_datareader.GetString(2),
                        CName = sqlite_datareader.GetString(5),
                        JobNr = sqlite_datareader.GetString(1),
                        EditorName = sqlite_datareader.GetString(8),
                        Status = sqlite_datareader.GetString(6),
                        DateCreated = sqlite_datareader.GetString(3),
                    };

                    cardChecks.Add(cardCheckProcess);
                }
                return new ObservableCollection<CardCheckProcess>(cardChecks);
            }
        }
        catch (Exception ex)
        {
            LogWriter.CreateLogEntry(ex);
        }

        return null;
    }

    public async Task GetCardCheckReportFromMSSQLAsync(string id)
    {
        try
        {
            using SettingsReaderWriter settings = new SettingsReaderWriter();
            settings.ReadSettings();

            var tmpFilePath = settings.DefaultSettings.DefaultProjectOutputPath + "\\" + "downloadedReport.pdf";

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = serverName;
            builder.IntegratedSecurity = true;
            builder.InitialCatalog = dbName;
            builder.ConnectRetryInterval = 10;
            builder.ConnectRetryCount = 2;
            builder.ConnectTimeout = 30;
            builder.ApplicationName = serverName.Split('\\')[1];

            using SqlConnection sqlConnection = new SqlConnection(builder.ConnectionString);

            if (sqlConnection != null)
            {
                if (sqlConnection.State == System.Data.ConnectionState.Closed)
                {
                    await sqlConnection.OpenAsync();
                    IsConnected = true;
                }

                using SqlCommand sql_cmd = sqlConnection.CreateCommand();

                sql_cmd.CommandText = "SELECT [CC-ReportBase64] FROM " + OMNITABLENAME + " Where [CC-ID] = @id";

                sql_cmd.Parameters.AddWithValue("@id", id);

                // The reader needs to be executed with the SequentialAccess behavior to enable network streaming
                // Otherwise ReadAsync will buffer the entire BLOB into memory which can cause scalability issues or even OutOfMemoryExceptions
                using SqlDataReader reader = await sql_cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

                if (await reader.ReadAsync())
                {
                    if (!(await reader.IsDBNullAsync(0)))
                    {
                        using FileStream file = File.Create(tmpFilePath);

                        var data = ConvertFromBase64(reader.GetString(0));

                        await file.WriteAsync(data, 0, data.Length);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogWriter.CreateLogEntry(ex);
            IsConnected = false;
        }
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
            LogWriter.CreateLogEntry(e);
        }
    }

    private async Task CreateTableAsync(SqlConnection conn, string dbName, string tableName)
    {
        try
        {
            var dbCmd = conn.CreateCommand();
            dbCmd.CommandText = "USE " + dbName + tableName;
            await dbCmd.ExecuteNonQueryAsync();

            conn.Close();
        }
        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e);
        }
    }

    public async Task InsertData(string key, FileStream fs)
    {
        try
        {
            CardChecks = new List<CardCheckProcess>();

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = serverName;
            builder.IntegratedSecurity = true;
            builder.InitialCatalog = dbName;
            builder.ConnectRetryInterval = 10;
            builder.ConnectRetryCount = 2;
            builder.ConnectTimeout = 30;
            builder.ApplicationName = serverName.Split('\\')[1];

            using SqlConnection sqlConnection = new SqlConnection(builder.ConnectionString);

            if (sqlConnection != null)
            {
                if (sqlConnection.State == System.Data.ConnectionState.Closed)
                {
                    await sqlConnection.OpenAsync();
                    IsConnected = true;
                }

                using SqlCommand sql_cmd = sqlConnection.CreateCommand();

                sql_cmd.CommandText = "UPDATE " + OMNITABLENAME + " SET [CC-ReportBase64] = @data Where [CC-ID] = @id";

                sql_cmd.Parameters.AddWithValue("@id", key);

                // Add a parameter which uses the FileStream we just opened
                // Size is set to -1 to indicate "MAX"

                sql_cmd.Parameters.Add("@data", SqlDbType.NText, -1).Value = ConvertToBase64(fs);

                // Send the data to the server asynchronously
                await sql_cmd.ExecuteScalarAsync();

                sqlConnection.Close();

            }
        }
        catch (Exception ex)
        {
            LogWriter.CreateLogEntry(ex);
            IsConnected = false;
        }
    }

    public async Task InsertData(string key, string value)
    {
        try
        {
            await InsertData("CC-Status", key, value);
        }
        catch (Exception ex)
        {
            LogWriter.CreateLogEntry(ex);
            IsConnected = false;
        }
    }

    public async Task InsertData(string columnID, string key, string value)
    {
        try
        {
            CardChecks = new List<CardCheckProcess>();

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = serverName;
            builder.IntegratedSecurity = true;
            builder.InitialCatalog = dbName;
            builder.ConnectRetryInterval = 10;
            builder.ConnectRetryCount = 2;
            builder.ConnectTimeout = 30;
            builder.PacketSize = 512;
            builder.ApplicationName = serverName.Split('\\')[1];

            using SqlConnection sqlConnection = new SqlConnection(builder.ConnectionString);

            if (sqlConnection != null)
            {
                if (sqlConnection.State == System.Data.ConnectionState.Closed)
                {
                    await sqlConnection.OpenAsync();
                    IsConnected = true;
                }

                using SqlCommand sql_cmd = sqlConnection.CreateCommand();

                sql_cmd.CommandText = string.Format("UPDATE {0} SET [{1}] = @status Where [CC-ID] = @id", OMNITABLENAME, columnID);

                sql_cmd.Parameters.AddWithValue("@id", key);
                sql_cmd.Parameters.AddWithValue("@status", value);

                sql_cmd.ExecuteNonQuery();
                sqlConnection.Close();
            }
        }
        catch (Exception ex)
        {
            LogWriter.CreateLogEntry(ex);
            IsConnected = false;
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
            LogWriter.CreateLogEntry(ex);
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

    private string ConvertToBase64(Stream stream)
    {
        byte[] bytes;
        using (var memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            bytes = memoryStream.ToArray();
        }

        return Convert.ToBase64String(bytes);
    }

    private byte[] ConvertFromBase64(string base64)
    {
        return Convert.FromBase64String(base64);
    }

    protected void Dispose(bool disposing)
    {
        if (!_disposed)
        {
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