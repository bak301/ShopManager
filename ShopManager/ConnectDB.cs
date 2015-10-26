using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ShopManager {
    class ConnectDB {
        private MySqlConnection con;
        private MySqlCommand cmd;
        private MySqlDataReader reader;

        public ConnectDB() {
            connectToDB();
        }

        // --------------- Connect to the database ------------------
        public void connectToDB() {
            string server = "Server=localhost;Database=shopmanger;Uid=root;Password=4;";
            con = new MySqlConnection(server);

            try {
                con.Open();
                cmd = con.CreateCommand();
                Console.WriteLine("Connected to database !");
            } catch (MySqlException e) {
                Console.WriteLine(e.StackTrace);
            }
        }

        // -------------------- Get table reader ---------------
        private List<string> columnList(string tablename) {
            List<string> list = new List<string>();
            try {
                cmd.CommandText = "select * from " + tablename + ";";
                reader = cmd.ExecuteReader();
                int colCount = reader.FieldCount;
                for (int i= 1; i < colCount; i++) {
                    list.Add(reader.GetName(i));
                }
                reader.Close();
                return list;
            } catch (MySqlException) {
                throw;
            }
        }

        // ------------------- Show all data in tables --------------
        public void showAll(string tableName) {
            tableName = tableName.ToLower();
            Console.Clear();
            try {
                List<string> listCol = columnList(tableName);
                listCol.Insert(0, "ID");
                cmd.CommandText = "select * from " + tableName + ";";
                reader = cmd.ExecuteReader();
                if (reader.HasRows) {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n\n\t" + string.Join("\t|\t", listCol.Select(x=>x.ToUpper()).ToList()));                   
                    Console.WriteLine("----------------------------------------------------------------------");
                    Console.ResetColor();
                    while (reader.Read()) {
                        string buildResult = "\t";
                        foreach (string name in listCol) {
                            switch (reader.GetFieldType(reader.GetOrdinal(name)).Name.ToLower()) {
                                case "string":
                                    buildResult += reader.GetString(reader.GetOrdinal(name)) + "\t|\t";
                                    break;
                                case "int32":
                                    buildResult += reader.GetInt16(reader.GetOrdinal(name)) + "\t|\t";
                                    break;
                                case "double":
                                    buildResult += reader.GetDouble(reader.GetOrdinal(name)) + "\t|\t";
                                    break;
                            }
                        }
                        Console.WriteLine(buildResult);
                    }
                    reader.Close();
                    Console.ReadKey();
                } else {
                    Console.WriteLine("Nothing founds!");
                    Console.ReadKey();
                }
            } catch (MySqlException) {
                throw;
            }
        }

        // ----------- Interact with Table ---------------------
        public void add<T>(T item) {
            cmd.Parameters.Clear();
            string itemClass = item.GetType().Name.ToLower();

            List<string> listCol = columnList(itemClass);
            List<string> mySqlParameter = listCol.Select(x => "@" + x).ToList();

            string propList = string.Join(",",listCol);
            string paraList = string.Join(",", mySqlParameter);

            string statement = "insert into " + itemClass + "(" + propList + ") values (" + paraList + ");";
            try {
                Console.WriteLine("Execute query : " + statement);
                cmd.CommandText = statement;
                cmd.Prepare();

                foreach (string colName in listCol) {
                    var prop = item.GetType().GetProperty(colName);
                    Console.WriteLine(colName, prop.PropertyType);
                    cmd.Parameters.AddWithValue("@" + colName, prop.GetValue(item,null));
                }

                cmd.ExecuteNonQuery();
                Console.WriteLine("Add item {0} success", item);
                Console.ReadKey();
            } catch (MySqlException) {
                throw;
            }
        }

        public void update<T>(T newItem, int ID) {
            cmd.Parameters.Clear();
            string itemClass = newItem.GetType().Name.ToLower();
            List<string> listCol = columnList(itemClass);

            string paralist = string.Join(",", listCol.Select(x => x + "=@" + x).ToList());
            string statement = "update " + itemClass + " set " + paralist + " where ID=@id;";
            try {
                cmd.CommandText = statement;
                cmd.Prepare();

                foreach (string colName in listCol) {
                    var prop = newItem.GetType().GetProperty(colName);
                    Console.WriteLine(prop.Name);
                    cmd.Parameters.AddWithValue("@" + colName, prop.GetValue(newItem, null));
                }
                cmd.Parameters.AddWithValue("@id", ID);
                cmd.ExecuteNonQuery();
                Console.WriteLine("Update item {0} to ID {1} success !", newItem, ID);
            } catch (MySqlException) {

                throw;
            }
        }

        public void delete(string itemClass, int ID) {
            cmd.Parameters.Clear();
            string statement = "delete from " + itemClass + " where id = @id;";
            try {
                cmd.CommandText = statement;
                cmd.Prepare();

                cmd.Parameters.AddWithValue("@id", ID);
                cmd.ExecuteNonQuery();
                Console.WriteLine("Delete {1} {0} completed! ", ID, itemClass);
            } catch (MySqlException) {
                throw;
            }
        }

        public void AnalyzeLast5() {
            new Analysis(cmd, reader).analysisTop5();
        }
    }
}
