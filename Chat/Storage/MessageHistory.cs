using System.Data.SQLite;

namespace Chat.Storage;

public static class MessageHistory
{
    public static string ConnectionString = @"Data Source=D:\Univ\COURSACHS\RIS\Chat\Chat\Db\messages.db;Version=3;Pooling=True;";

    public const string CreateTableQuery = """
                                            CREATE TABLE IF NOT EXISTS MessageHistory (
                                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                Username TEXT NOT NULL,
                                                Message TEXT NOT NULL,
                                                Image BLOB,
                                                Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                                            );
                                            """;

    private const string AddImageColumnQuery = @"
            ALTER TABLE MessageHistory ADD COLUMN Image BLOB;";

    private const string InsertMessageQuery = @"
            INSERT INTO MessageHistory (Username, Message, Image) VALUES (@Username, @Message, @Image);";

    private const string GetMessagesQuery = @"
            SELECT Message, Image FROM MessageHistory WHERE Username = @Username ORDER BY Timestamp;";

    static MessageHistory()
    {
        InitializeDatabase();
    }
    
    private static readonly object DbLock = new();

    private static void ExecuteWithLock(Action action)
    {
        lock (DbLock)
        {
            action();
        }
    }

    private static void InitializeDatabase()
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(CreateTableQuery, connection);
        command.ExecuteNonQuery();
        
        using var checkCommand = new SQLiteCommand("PRAGMA table_info(MessageHistory);", connection);
        using var reader = checkCommand.ExecuteReader();
        var hasImageColumn = false;

        while (reader.Read())
        {
            if (reader["name"].ToString() != "Image") continue;
            hasImageColumn = true;
            break;
        }

        if (hasImageColumn) return;
        using var alterCommand = new SQLiteCommand(AddImageColumnQuery, connection);
        alterCommand.ExecuteNonQuery();
    }

    public static void LogMessage(string username, string message, byte[]? image = null)
    {
        ExecuteWithLock(() =>
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            using var command = new SQLiteCommand(InsertMessageQuery, connection);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Message", message);
            command.Parameters.AddWithValue("@Image", image ?? (object)DBNull.Value);

            command.ExecuteNonQuery();
        });
    }


    public static List<(string Message, string? Base64Image)> GetMessages(string username)
    {
        var messages = new List<(string, string?)>();

        ExecuteWithLock(() =>
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            using var command = new SQLiteCommand(GetMessagesQuery, connection);
            command.Parameters.AddWithValue("@Username", username);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var message = reader["Message"].ToString()!;
                var base64Image =
                    reader["Image"] is byte[] image ? Convert.ToBase64String(image) : null; // Конвертируем в Base64
                messages.Add((message, base64Image));
            }
        });

        return messages;
    }

}
