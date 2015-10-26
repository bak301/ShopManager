using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ShopManager {
    class Analysis {
        private MySqlCommand cmd { get; set; }
        private MySqlDataReader reader { get; set; }
        private string buildup ="";
        private List<string> last5List, last5NoSpace;
        public Analysis(MySqlCommand c, MySqlDataReader r) {
            this.cmd = c;
            this.reader = r;
            dropTempView();
        }

        public void analysisTop5() {
            // Clear the command parameters before do anything
            cmd.Parameters.Clear();

            // Start the create alot of views process
            createList5();
            createExtended();
            extendedSum();
            replaceNullwith0();
            finalize();

            // Finally, output it
            showToConsole();
            Console.ReadKey();
        }

        private void createList5() {
            // ---------- Get the last 5 product -------------
            cmd.CommandText = "select name from last5";
            reader = cmd.ExecuteReader();
            last5List = new List<string>();

            // ------------ Add name in a list to iterate later
            while (reader.Read()) {
                last5List.Add(reader.GetString("name"));
            }

            last5NoSpace = last5List.Select(x => x.Replace(" ", "")).ToList();
            reader.Close();
        }

        private void createExtended() {
            // ------------ Iterate through list to build column name dynamically --------------
            buildup += string.Join(" , ", last5List.Select(x =>
                "case when name = \"" + x + "\" then TongSP end as `" + x.Replace(" ", "") + "`"
            ));

            string createExtendedView = "create view extended as select `loai`, "
                + buildup + " from analysis;";
            cmd.CommandText = createExtendedView;
            //Console.WriteLine("Execute query : " + createExtendedView);
            cmd.ExecuteNonQuery();
        }

        private void extendedSum() {
            // ----------- After view extended has been created, we will simplify it -------------
            buildup = "";
            buildup += string.Join(" , ", last5NoSpace.Select(x => "sum(`" + x + "`) as " + "`" + x + "`"));
            string createExtendedView2 = "create view extended2 as select loai, " + buildup + " from extended group by loai";
            cmd.CommandText = createExtendedView2;
            //Console.WriteLine("Execute query : " + createExtendedView2);
            cmd.ExecuteNonQuery();
        }

        private void replaceNullwith0() {
            // ------------ Now we will replace NULL value with 0 -------------
            buildup = "";
            buildup += string.Join(" , ", last5NoSpace.Select(x => "coalesce(" + x + ",\"\") as " + x));
            string createExtendedViewPretty = "create view extendedPretty as select loai, " + buildup + " from extended2 ";
            cmd.CommandText = createExtendedViewPretty;
            cmd.ExecuteNonQuery();
        }

        private void finalize() {
            // ------------ Last query, we gonna get the sum of product column into one ---------
            buildup = ""; string buildup2 = "";
            buildup += string.Join(" + ", last5NoSpace.Select(x => "`" + x + "`")) + " as `Total`";
            buildup2 += string.Join(" , ", last5NoSpace);
            string createExtendedViewFinal = "create view final as select loai, " + buildup + " , " + buildup2 + " from extendedPretty";
            cmd.CommandText = createExtendedViewFinal;
            cmd.ExecuteNonQuery();
        }

        private void showToConsole() {
            Console.Clear();

            //---------------- Get a list of column name ---------------
            cmd.CommandText = "select * from final;";
            reader = cmd.ExecuteReader();
            List<string> columnList = new List<string>();
            for (int i = 0; i< reader.FieldCount; i++) {
                columnList.Add(reader.GetName(i));
            }

            //---------------- Start output to console -----------------
            if (reader.HasRows) {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n\n\t" + string.Join("\t|\t", columnList.Select(x => x.ToUpper()).ToList()));
                Console.WriteLine("--------------------------------------------------------------------------------------------------");
                Console.ResetColor();
                while (reader.Read()) {
                    buildup = "";
                    for (int i = 1; i< columnList.Count; i++) {
                        try {
                            buildup += "\t|\t" + reader.GetInt32(i);
                        } catch (Exception) {
                            buildup += "\t|\t" + reader.GetString(i);
                        }
                    }
                    Console.WriteLine("\t" + reader.GetString(0) + buildup);
                }
            }
            reader.Close();

            // --------------- Delete all the view ---------------------
            dropTempView();
        }

        private void dropTempView() {
            cmd.CommandText = "drop view if exists extended, extended2, extendedPretty, final";
            cmd.ExecuteNonQuery();
        }
    }
}
