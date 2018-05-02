using SQLitePCL;
using System;
using System.Diagnostics;

namespace DataAccessLibrary {
    public static class DataAccess {
        private static string create = @"CREATE TABLE IF NOT EXISTS MyList(Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
                                                                            "Title VARCHAR(140)," +
                                                                            "Detail VARCHAR(140)," +
                                                                            "Date VARCHAR(140)," +
                                                                            "Completed INTEGER," + 
                                                                            "Image BLOB)";
        private static string insert = @"INSERT INTO MyList(Id, Title, Detail, Date, Completed, Image) " +
                                        "VALUES (?, ?, ?, ?, ?, ?)";
        private static string delete = @"DELETE FROM MyList " +
                                        "WHERE Id = ?";
        private static string update = @"UPDATE MyList " +
                                        "SET Title = ?, Detail = ?, Date = ?, Completed = ?, Image = ? "+ 
                                        "WHERE Id = ?";
        private static string vagueQuery = @"SELECT * " +
                                            "FROM MyList " + 
                                            "WHERE Title LIKE ? OR Detail LIKE ? OR Date LIKE ?";
        private static string titleQuery = @"SELECT * " +
                                            "FROM MyList " +
                                            "WHERE Title LIKE ?";
        private static string dateQuery = @"SELECT * " +
                                            "FROM MyList " +
                                            "WHERE Date LIKE ?";
        private static string seperate = new string('-', 80);

        public static SQLiteConnection connection;

        public static void InitializeDatabase() {
            connection = new SQLiteConnection("MyList.db");
            try {
                using (var statement = connection.Prepare(create)) {
                    statement.Step();
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        public static void AddData(long id, string title, string detail, string date, int completed, byte[] image) {
            try {
                using (var statement = connection.Prepare(insert)) {
                    statement.Bind(1, id);
                    statement.Bind(2, title);
                    statement.Bind(3, detail);
                    statement.Bind(4, date);
                    statement.Bind(5, completed);
                    statement.Bind(6, image);
                    statement.Step();
                }
            } catch(Exception ex) {
                Debug.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        public static void DeleteData(long id) {
            try {
                using (var statement = connection.Prepare(delete)) {
                    statement.Bind(1, id);
                    statement.Step();
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        public static void UpdateDate(long id, string title, string detail, string date, int completed, byte[] image) {
            try {
                using (var statement = connection.Prepare(update)) {
                    statement.Bind(1, title);
                    statement.Bind(2, detail);
                    statement.Bind(3, date);
                    statement.Bind(4, completed);
                    statement.Bind(5, image);
                    statement.Bind(6, id);
                    statement.Step();
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        public static string VagueQueryData(string info) {
            
            string result = "";
            using (var statement = connection.Prepare(vagueQuery)) {
                statement.Bind(1, "%" + info + "%");
                statement.Bind(2, "%" + info + "%");
                statement.Bind(3, "%" + info + "%");
                while (statement.Step() == SQLiteResult.ROW) {
                    string IsCompleted = (Int64)statement[4] == 1 ? "Yes" : "No";
                    result += seperate + "\n";
                    result += "Id\t\t: " + statement[0] + "\n";
                    result += "Title\t\t: " + statement[1] + "\n";
                    result += "Detail\t\t: " + statement[2] + "\n";
                    result += "Date\t\t: " + statement[3] + "\n";
                    result += "Completed\t: " + IsCompleted + "\n";
                    result += seperate + "\n";
                }
            }
            if (result == "") result += "\tNo Matched Item!\n";
            return result;
        }

        public static string TitleQueryData(string info) {
            string result = "";
            using (var statement = connection.Prepare(titleQuery)) {
                statement.Bind(1, "%" + info + "%");
                while (statement.Step() == SQLiteResult.ROW) {
                    string IsCompleted = (Int64)statement[4] == 1 ? "Yes" : "No";
                    result += seperate + "\n";
                    result += "Id\t\t: " + statement[0] + "\n";
                    result += "Title\t\t: " + statement[1] + "\n";
                    result += "Detail\t\t: " + statement[2] + "\n";
                    result += "Date\t\t: " + statement[3] + "\n";
                    result += "Completed\t: " + IsCompleted + "\n";
                    result += seperate + "\n";
                }
            }
            if (result == "") result += "\tNo Matched Item!\n";
            return result;
        }

        public static string DateQueryData(string info) {
            string result = "";
            using (var statement = connection.Prepare(dateQuery)) {
                statement.Bind(1, "%" + info + "%");
                while (statement.Step() == SQLiteResult.ROW) {
                    string IsCompleted = (Int64)statement[4] == 1 ? "Yes" : "No";
                    result += seperate + "\n";
                    result += "Id\t\t: " + statement[0] + "\n";
                    result += "Title\t\t: " + statement[1] + "\n";
                    result += "Detail\t\t: " + statement[2] + "\n";
                    result += "Date\t\t: " + statement[3] + "\n";
                    result += "Completed\t: " + IsCompleted + "\n";
                    result += seperate + "\n";
                }
            }
            if (result == "") result += "\tNo Matched Item!\n";
            return result;
        }
    }
}
