using BookManager.Models;
using Microsoft.Data.Sqlite;
using System.Data;
using System.IO;

namespace BookManager
{
    public class DbHelper
    {
        private static readonly string _dbPath = Path.Combine(Application.StartupPath, "library.db");

        public static void InitializeDatabase()
        {
            try
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();

                string createUsersTable = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        Password TEXT NOT NULL,
                        Role TEXT NOT NULL DEFAULT 'user',
                        Name TEXT NOT NULL,
                        Phone TEXT,
                        CreatedAt DATETIME NOT NULL
                    )";

                string createBooksTable = @"
                    CREATE TABLE IF NOT EXISTS Books (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Isbn TEXT NOT NULL UNIQUE,
                        Title TEXT NOT NULL,
                        Author TEXT NOT NULL,
                        Publisher TEXT NOT NULL,
                        Price DECIMAL NOT NULL,
                        Quantity INTEGER NOT NULL,
                        AvailableQuantity INTEGER NOT NULL,
                        Category TEXT NOT NULL,
                        PublishDate DATETIME NOT NULL,
                        Description TEXT
                    )";

                string createBorrowRecordsTable = @"
                    CREATE TABLE IF NOT EXISTS BorrowRecords (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        BookId INTEGER NOT NULL,
                        BorrowDate DATETIME NOT NULL,
                        DueDate DATETIME NOT NULL,
                        ReturnDate DATETIME,
                        Status TEXT NOT NULL DEFAULT 'borrowed',
                        FOREIGN KEY (UserId) REFERENCES Users(Id),
                        FOREIGN KEY (BookId) REFERENCES Books(Id)
                    )";

                ExecuteNonQuery(createUsersTable);
                ExecuteNonQuery(createBooksTable);
                ExecuteNonQuery(createBorrowRecordsTable);

                CreateAdminIfNotExists();
                InsertSampleBooks();
            }
            catch (Exception ex)
            {
                throw new Exception("数据库初始化失败: " + ex.Message);
            }
        }

        private static void CreateAdminIfNotExists()
        {
            if (!UserExists("admin"))
            {
                string insertAdmin = @"
                    INSERT INTO Users (Username, Password, Role, Name, Phone, CreatedAt)
                    VALUES ('admin', 'admin123', 'admin', '管理员', '13800138000', datetime('now'))";
                ExecuteNonQuery(insertAdmin);
            }
        }

        private static void InsertSampleBooks()
        {
            if (GetBookCount() == 0)
            {
                string[] books = {
                    "INSERT INTO Books (Isbn, Title, Author, Publisher, Price, Quantity, AvailableQuantity, Category, PublishDate, Description) VALUES ('9787040167369', '高等数学', '同济大学', '高等教育出版社', 56.00, 10, 10, '数学', '2014-07-01', '高等数学教材')",
                    "INSERT INTO Books (Isbn, Title, Author, Publisher, Price, Quantity, AvailableQuantity, Category, PublishDate, Description) VALUES ('9787111213826', 'C程序设计', '谭浩强', '机械工业出版社', 39.00, 8, 8, '计算机', '2017-07-01', 'C语言入门教材')",
                    "INSERT INTO Books (Isbn, Title, Author, Publisher, Price, Quantity, AvailableQuantity, Category, PublishDate, Description) VALUES ('9787302227737', 'Java程序设计', 'Y.Daniel Liang', '清华大学出版社', 69.00, 6, 6, '计算机', '2011-06-01', 'Java编程入门')",
                    "INSERT INTO Books (Isbn, Title, Author, Publisher, Price, Quantity, AvailableQuantity, Category, PublishDate, Description) VALUES ('9787508616812', '经济学原理', '曼昆', '北京大学出版社', 64.00, 5, 5, '经济', '2009-07-01', '经济学经典教材')",
                    "INSERT INTO Books (Isbn, Title, Author, Publisher, Price, Quantity, AvailableQuantity, Category, PublishDate, Description) VALUES ('9787020042408', '红楼梦', '曹雪芹', '人民文学出版社', 42.00, 12, 12, '文学', '2008-07-01', '中国古典名著')"
                };

                foreach (string book in books)
                {
                    ExecuteNonQuery(book);
                }
            }
        }

        private static bool UserExists(string username)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        private static int GetBookCount()
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            string query = "SELECT COUNT(*) FROM Books";
            using var command = new SqliteCommand(query, connection);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public static void ExecuteNonQuery(string sql, params SqliteParameter[] parameters)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddRange(parameters);
            command.ExecuteNonQuery();
        }

        public static object ExecuteScalar(string sql, params SqliteParameter[] parameters)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddRange(parameters);
            return command.ExecuteScalar();
        }

        public static DataTable ExecuteQuery(string sql, params SqliteParameter[] parameters)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddRange(parameters);
            using var reader = command.ExecuteReader();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public static User? GetUserByUsername(string username)
        {
            string query = "SELECT * FROM Users WHERE Username = @Username";
            var dt = ExecuteQuery(query, new SqliteParameter("@Username", username));
            
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return new User
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Username = row["Username"].ToString() ?? string.Empty,
                    Password = row["Password"].ToString() ?? string.Empty,
                    Role = row["Role"].ToString() ?? "user",
                    Name = row["Name"].ToString() ?? string.Empty,
                    Phone = row["Phone"]?.ToString() ?? string.Empty,
                    CreatedAt = DateTime.Parse(row["CreatedAt"].ToString() ?? DateTime.Now.ToString())
                };
            }
            return null;
        }

        public static bool AddUser(User user)
        {
            try
            {
                string sql = @"
                    INSERT INTO Users (Username, Password, Role, Name, Phone, CreatedAt)
                    VALUES (@Username, @Password, @Role, @Name, @Phone, @CreatedAt)";
                
                ExecuteNonQuery(sql,
                    new SqliteParameter("@Username", user.Username),
                    new SqliteParameter("@Password", user.Password),
                    new SqliteParameter("@Role", user.Role),
                    new SqliteParameter("@Name", user.Name),
                    new SqliteParameter("@Phone", user.Phone),
                    new SqliteParameter("@CreatedAt", user.CreatedAt)
                );
                return true;
            }
            catch (SqliteException)
            {
                return false;
            }
        }

        public static List<User> GetAllUsers()
        {
            var users = new List<User>();
            string query = "SELECT * FROM Users ORDER BY Id DESC";
            var dt = ExecuteQuery(query);
            
            foreach (DataRow row in dt.Rows)
            {
                users.Add(new User
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Username = row["Username"].ToString() ?? string.Empty,
                    Password = row["Password"].ToString() ?? string.Empty,
                    Role = row["Role"].ToString() ?? "user",
                    Name = row["Name"].ToString() ?? string.Empty,
                    Phone = row["Phone"]?.ToString() ?? string.Empty,
                    CreatedAt = DateTime.Parse(row["CreatedAt"].ToString() ?? DateTime.Now.ToString())
                });
            }
            return users;
        }

        public static bool UpdateUser(User user)
        {
            try
            {
                string sql = @"
                    UPDATE Users SET Username = @Username, Password = @Password, Role = @Role, 
                        Name = @Name, Phone = @Phone WHERE Id = @Id";
                
                ExecuteNonQuery(sql,
                    new SqliteParameter("@Username", user.Username),
                    new SqliteParameter("@Password", user.Password),
                    new SqliteParameter("@Role", user.Role),
                    new SqliteParameter("@Name", user.Name),
                    new SqliteParameter("@Phone", user.Phone),
                    new SqliteParameter("@Id", user.Id)
                );
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DeleteUser(int id)
        {
            try
            {
                ExecuteNonQuery("DELETE FROM Users WHERE Id = @Id", new SqliteParameter("@Id", id));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static List<Book> SearchBooks(string keyword, string category = "", string author = "", string publisher = "")
        {
            var books = new List<Book>();
            string query = "SELECT * FROM Books WHERE 1=1";
            var parameters = new List<SqliteParameter>();

            if (!string.IsNullOrEmpty(keyword))
            {
                query += " AND (Title LIKE @Keyword OR Isbn LIKE @Keyword)";
                parameters.Add(new SqliteParameter("@Keyword", "%" + keyword + "%"));
            }
            if (!string.IsNullOrEmpty(category))
            {
                query += " AND Category = @Category";
                parameters.Add(new SqliteParameter("@Category", category));
            }
            if (!string.IsNullOrEmpty(author))
            {
                query += " AND Author LIKE @Author";
                parameters.Add(new SqliteParameter("@Author", "%" + author + "%"));
            }
            if (!string.IsNullOrEmpty(publisher))
            {
                query += " AND Publisher LIKE @Publisher";
                parameters.Add(new SqliteParameter("@Publisher", "%" + publisher + "%"));
            }

            query += " ORDER BY Id DESC";
            var dt = ExecuteQuery(query, parameters.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                books.Add(new Book
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Isbn = row["Isbn"].ToString() ?? string.Empty,
                    Title = row["Title"].ToString() ?? string.Empty,
                    Author = row["Author"].ToString() ?? string.Empty,
                    Publisher = row["Publisher"].ToString() ?? string.Empty,
                    Price = Convert.ToDecimal(row["Price"]),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    AvailableQuantity = Convert.ToInt32(row["AvailableQuantity"]),
                    Category = row["Category"].ToString() ?? string.Empty,
                    PublishDate = DateTime.Parse(row["PublishDate"].ToString() ?? DateTime.Now.ToString()),
                    Description = row["Description"]?.ToString() ?? string.Empty
                });
            }
            return books;
        }

        public static List<Book> GetAllBooks()
        {
            return SearchBooks("");
        }

        public static bool AddBook(Book book)
        {
            try
            {
                string sql = @"
                    INSERT INTO Books (Isbn, Title, Author, Publisher, Price, Quantity, 
                        AvailableQuantity, Category, PublishDate, Description)
                    VALUES (@Isbn, @Title, @Author, @Publisher, @Price, @Quantity, 
                        @AvailableQuantity, @Category, @PublishDate, @Description)";
                
                ExecuteNonQuery(sql,
                    new SqliteParameter("@Isbn", book.Isbn),
                    new SqliteParameter("@Title", book.Title),
                    new SqliteParameter("@Author", book.Author),
                    new SqliteParameter("@Publisher", book.Publisher),
                    new SqliteParameter("@Price", book.Price),
                    new SqliteParameter("@Quantity", book.Quantity),
                    new SqliteParameter("@AvailableQuantity", book.AvailableQuantity),
                    new SqliteParameter("@Category", book.Category),
                    new SqliteParameter("@PublishDate", book.PublishDate),
                    new SqliteParameter("@Description", book.Description)
                );
                return true;
            }
            catch (SqliteException)
            {
                return false;
            }
        }

        public static bool UpdateBook(Book book)
        {
            try
            {
                string sql = @"
                    UPDATE Books SET Isbn = @Isbn, Title = @Title, Author = @Author, 
                        Publisher = @Publisher, Price = @Price, Quantity = @Quantity, 
                        AvailableQuantity = @AvailableQuantity, Category = @Category, 
                        PublishDate = @PublishDate, Description = @Description WHERE Id = @Id";
                
                ExecuteNonQuery(sql,
                    new SqliteParameter("@Isbn", book.Isbn),
                    new SqliteParameter("@Title", book.Title),
                    new SqliteParameter("@Author", book.Author),
                    new SqliteParameter("@Publisher", book.Publisher),
                    new SqliteParameter("@Price", book.Price),
                    new SqliteParameter("@Quantity", book.Quantity),
                    new SqliteParameter("@AvailableQuantity", book.AvailableQuantity),
                    new SqliteParameter("@Category", book.Category),
                    new SqliteParameter("@PublishDate", book.PublishDate),
                    new SqliteParameter("@Description", book.Description),
                    new SqliteParameter("@Id", book.Id)
                );
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DeleteBook(int id)
        {
            try
            {
                ExecuteNonQuery("DELETE FROM Books WHERE Id = @Id", new SqliteParameter("@Id", id));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool BorrowBook(int userId, int bookId)
        {
            try
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();
                using var transaction = connection.BeginTransaction();

                string checkBook = "SELECT AvailableQuantity FROM Books WHERE Id = @BookId";
                using var command = new SqliteCommand(checkBook, connection, transaction);
                command.Parameters.AddWithValue("@BookId", bookId);
                int available = Convert.ToInt32(command.ExecuteScalar());

                if (available <= 0)
                {
                    transaction.Rollback();
                    return false;
                }

                string updateBook = "UPDATE Books SET AvailableQuantity = AvailableQuantity - 1 WHERE Id = @BookId";
                using var updateCommand = new SqliteCommand(updateBook, connection, transaction);
                updateCommand.Parameters.AddWithValue("@BookId", bookId);
                updateCommand.ExecuteNonQuery();

                string insertRecord = @"
                    INSERT INTO BorrowRecords (UserId, BookId, BorrowDate, DueDate, Status)
                    VALUES (@UserId, @BookId, @BorrowDate, @DueDate, 'borrowed')";
                using var insertCommand = new SqliteCommand(insertRecord, connection, transaction);
                insertCommand.Parameters.AddWithValue("@UserId", userId);
                insertCommand.Parameters.AddWithValue("@BookId", bookId);
                insertCommand.Parameters.AddWithValue("@BorrowDate", DateTime.Now);
                insertCommand.Parameters.AddWithValue("@DueDate", DateTime.Now.AddDays(30));
                insertCommand.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ReturnBook(int recordId)
        {
            try
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();
                using var transaction = connection.BeginTransaction();

                string getBookId = "SELECT BookId FROM BorrowRecords WHERE Id = @RecordId";
                using var command = new SqliteCommand(getBookId, connection, transaction);
                command.Parameters.AddWithValue("@RecordId", recordId);
                int bookId = Convert.ToInt32(command.ExecuteScalar());

                string updateBook = "UPDATE Books SET AvailableQuantity = AvailableQuantity + 1 WHERE Id = @BookId";
                using var updateCommand = new SqliteCommand(updateBook, connection, transaction);
                updateCommand.Parameters.AddWithValue("@BookId", bookId);
                updateCommand.ExecuteNonQuery();

                string updateRecord = @"
                    UPDATE BorrowRecords SET ReturnDate = @ReturnDate, Status = 'returned' 
                    WHERE Id = @RecordId";
                using var insertCommand = new SqliteCommand(updateRecord, connection, transaction);
                insertCommand.Parameters.AddWithValue("@ReturnDate", DateTime.Now);
                insertCommand.Parameters.AddWithValue("@RecordId", recordId);
                insertCommand.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static List<BorrowRecord> GetBorrowRecords(int? userId = null, string status = "")
        {
            var records = new List<BorrowRecord>();
            string query = @"
                SELECT br.*, u.Name as UserName, b.Title as BookTitle
                FROM BorrowRecords br
                JOIN Users u ON br.UserId = u.Id
                JOIN Books b ON br.BookId = b.Id
                WHERE 1=1";
            
            var parameters = new List<SqliteParameter>();

            if (userId.HasValue)
            {
                query += " AND br.UserId = @UserId";
                parameters.Add(new SqliteParameter("@UserId", userId.Value));
            }
            if (!string.IsNullOrEmpty(status))
            {
                query += " AND br.Status = @Status";
                parameters.Add(new SqliteParameter("@Status", status));
            }

            query += " ORDER BY br.BorrowDate DESC";
            var dt = ExecuteQuery(query, parameters.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                records.Add(new BorrowRecord
                {
                    Id = Convert.ToInt32(row["Id"]),
                    UserId = Convert.ToInt32(row["UserId"]),
                    BookId = Convert.ToInt32(row["BookId"]),
                    BorrowDate = DateTime.Parse(row["BorrowDate"].ToString() ?? DateTime.Now.ToString()),
                    DueDate = DateTime.Parse(row["DueDate"].ToString() ?? DateTime.Now.ToString()),
                    ReturnDate = row["ReturnDate"] != DBNull.Value ? DateTime.Parse(row["ReturnDate"].ToString()!) : null,
                    Status = row["Status"].ToString() ?? string.Empty,
                    UserName = row["UserName"].ToString() ?? string.Empty,
                    BookTitle = row["BookTitle"].ToString() ?? string.Empty
                });
            }
            return records;
        }

        public static List<string> GetCategories()
        {
            var categories = new List<string>();
            string query = "SELECT DISTINCT Category FROM Books ORDER BY Category";
            var dt = ExecuteQuery(query);
            
            foreach (DataRow row in dt.Rows)
            {
                categories.Add(row["Category"].ToString() ?? string.Empty);
            }
            return categories;
        }

        public static int GetOverdueCount()
        {
            string query = "SELECT COUNT(*) FROM BorrowRecords WHERE Status = 'borrowed' AND DueDate < @Today";
            return Convert.ToInt32(ExecuteScalar(query, new SqliteParameter("@Today", DateTime.Now)));
        }

        public static List<BorrowRecord> GetOverdueRecords()
        {
            return GetBorrowRecords(null, "borrowed").Where(r => r.DueDate < DateTime.Now).ToList();
        }
    }
}
