using System.Data.SQLite;
using Chat.Models;

namespace Chat.Storage;

public static class Authentication
{
    public static string ConnectionString = @"Data Source=D:\Univ\COURSACHS\RIS\Chat\Chat\Db\users.db;Version=3;Pooling=True;";

    private const string AuthQuery =
        "SELECT Username, Password, Role FROM Users WHERE Username = @Username AND Password = @Password;";
    
    private const string ExistsQuery =
        "SELECT COUNT(*) FROM Users WHERE Username = @Username;";

    public const string CreateTableQuery = """
                                           CREATE TABLE IF NOT EXISTS Users (
                                               Username TEXT PRIMARY KEY,
                                               Password TEXT NOT NULL,
                                               Role TEXT NOT NULL
                                           );
                                           """;

    private const string CheckUserExistsQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username;";

    private const string RegisterUserQuery =
        "INSERT INTO Users (Username, Password, Role) VALUES (@Username, @Password, @Role);";

    static Authentication()
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

        Register("admin", "1234", "admin");
    }
    
    public static bool Exists(string username)
    {
        var result = false;
        ExecuteWithLock(() =>
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            using var command = new SQLiteCommand(ExistsQuery, connection);
            command.Parameters.AddWithValue("@Username", username);

            result = (long)command.ExecuteScalar() != 0;
        });
        return result;
    }

    public static User? Authenticate(string username, string password)
    {
        User? result = null;
        ExecuteWithLock(() =>
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            using var command = new SQLiteCommand(AuthQuery, connection);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", password);

            using var reader = command.ExecuteReader();
            if (reader.Read())
                result = new User
                {
                    Username = reader["Username"].ToString()!,
                    Password = reader["Password"].ToString()!,
                    Role = reader["Role"].ToString()!
                };

        });
        return result;
    }

    public static bool Register(string username, string password, string role)
    {
        var result = false;
        ExecuteWithLock(() =>
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            using var checkCommand = new SQLiteCommand(CheckUserExistsQuery, connection);
            checkCommand.Parameters.AddWithValue("@Username", username);

            var userExists = (long)checkCommand.ExecuteScalar() > 0;
            if (userExists)
            {
                result = false;
                return;
            }

            using var registerCommand = new SQLiteCommand(RegisterUserQuery, connection);
            registerCommand.Parameters.AddWithValue("@Username", username);
            registerCommand.Parameters.AddWithValue("@Password", password);
            registerCommand.Parameters.AddWithValue("@Role", role);

            registerCommand.ExecuteNonQuery();
            result = true;
        });
        return result;
    }
}