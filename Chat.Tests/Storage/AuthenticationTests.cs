using System.Data.SQLite;
using Chat.Storage;
using NUnit.Framework;

namespace Chat.Tests.Storage;

    [TestFixture]
    public class AuthenticationTests
    {
        private const string TestConnectionString = @"Data Source=:memory:Version=3;";
        private SQLiteConnection _connection;

        [SetUp]
        public void SetUp()
        {
            // Настройка тестовой базы данных
            _connection = new SQLiteConnection(TestConnectionString);
            _connection.Open();

            // Создание таблицы Users
            using var command = new SQLiteCommand(Authentication.CreateTableQuery, _connection);
            command.ExecuteNonQuery();

            // Обновление строки подключения для тестов
            typeof(Authentication).GetField("ConnectionString", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.SetValue(null, TestConnectionString);
        }

        [TearDown]
        public void TearDown()
        {
            // Закрытие соединения
            _connection.Dispose();
        }

        [Test]
        public void Register_ShouldAddNewUser()
        {
            var result = Authentication.Register("testUser", "password", "user");

            Assert.That(result, Is.True);

            using var checkCommand = new SQLiteCommand("SELECT COUNT(*) FROM Users WHERE Username = 'testUser';", _connection);
            var count = (long)checkCommand.ExecuteScalar();
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void Register_ShouldNotAddDuplicateUser()
        {
            Authentication.Register("testUser", "password", "user");
            var result = Authentication.Register("testUser", "newpassword", "user");

            Assert.That(result, Is.False);
        }

        [Test]
        public void Authenticate_ShouldReturnUser_WhenCredentialsAreCorrect()
        {
            Authentication.Register("testUser", "password", "user");
            var user = Authentication.Authenticate("testUser", "password");

            Assert.That(user, Is.Not.Null);
            Assert.That(user.Username, Is.EqualTo("testUser"));
            Assert.That(user.Role, Is.EqualTo("user"));
        }

        [Test]
        public void Authenticate_ShouldReturnNull_WhenCredentialsAreInvalid()
        {
            Authentication.Register("testUser", "password", "user");
            var user = Authentication.Authenticate("testUser", "wrongpassword");

            Assert.That(user, Is.Null);
        }

        [Test]
        public void Exists_ShouldReturnTrue_IfUserExists()
        {
            Authentication.Register("testUser", "password", "user");
            var exists = Authentication.Exists("testUser");

            Assert.That(exists, Is.True);
        }

        [Test]
        public void Exists_ShouldReturnFalse_IfUserDoesNotExist()
        {
            var exists = Authentication.Exists("nonExistingUser");

            Assert.That(exists, Is.False);
        }
    }