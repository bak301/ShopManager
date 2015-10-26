using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ShopManager {
    class View {
        private ConnectDB con;
        private bool done = false;
        private int choice;
        static void Main(string[] args) {
            View v = new View();
            while (!v.done) {
                v.mainMenu();
            }
        }

        public View() {
            con = new ConnectDB();
        }

        private void chooseOption() {
            try {
                choice = int.Parse(Console.ReadLine());
            } catch (FormatException) {
                Console.WriteLine("Chi nhap cac so hien tren man hinh");
                System.Threading.Thread.Sleep(3000);
                chooseOption();
            }
        }

        private void mainMenu() {
            Console.Clear();
            Console.WriteLine("---------------------Welcome to Shop Manager------------------ \n"
                + "1. Choose table Product \n"
                + "2. Choose table Bill \n"
                + "3. Choose table ProductType \n"
                + "4. View last 5 item analysis\n" 
                + "5. Exit\n"
                + "Your choice: ");
            choice = 5;
            chooseOption();

            switch (choice) {
                case 1:
                    tableMenu("Product");
                    break;
                case 2:
                    tableMenu("Bill");
                    break;
                case 3:
                    tableMenu("ProductType");
                    break;
                case 4:
                    con.AnalyzeLast5();
                    break;
                case 5:
                    this.done = true;
                    break;
                default:
                    Console.WriteLine("Chi nhap cac so tu 1 den 4");
                    System.Threading.Thread.Sleep(5000);
                    mainMenu();
                    break;
            }
        }

        private void tableMenu(string tableName) {
            Console.Clear();
            Console.WriteLine("------------------------ Table Menu : " + tableName.ToUpper() + " ----------------------- \n"
                + "1. Show all data in the table \n"
                + "2. Add an item\n"
                + "3. Delete an item\n"
                + "4. Update an item\n"
                + "5. Back to main menu\n"
                + "Your choice: ");
            choice = 6;
            chooseOption();

            Console.Clear();
            switch (choice) {
                case 1:
                    con.showAll(tableName);
                    break;
                case 2:
                    Console.WriteLine("Add item {0} : ", tableName);
                    // ------------------ Create an instance of this the type -----------------
                    object newShopItem = Activator.CreateInstance(Type.GetType("ShopManager." + tableName));

                    // ------------- Get a list of shop's item properties ----------------
                    List<PropertyInfo> propName = newShopItem.GetType().GetProperties().ToList();
                    foreach (PropertyInfo propType in propName) {
                        Console.Write("Give {0} value : ", propType.Name);

                        // -------------- Convert user input data ( String ) to correct type of property --------------
                        var valueForProp = Convert.ChangeType(Console.ReadLine(),propType.PropertyType);

                        // -------------- Set value from user to property -----------------
                        newShopItem.GetType().GetProperty(propType.Name).SetValue(newShopItem, valueForProp);
                    }
                    con.add(newShopItem);
                    break;
                case 3:
                    Console.WriteLine("Delete an {0} item : ", tableName);
                    Console.WriteLine(" Input the ID of item you want to delete in table {0} : ( Note: You may want to see the table to know exactly which item you want to delete )", tableName);
                    int tempID = 0;
                    try {
                        tempID = int.Parse(Console.ReadLine());
                    } catch (FormatException) {
                        Console.WriteLine("Chi nhap so nguyen !");
                        System.Threading.Thread.Sleep(3000);
                        tableMenu(tableName);
                    }
                    con.delete(tableName, tempID);
                    break;
                case 4:
                    tempID = 0;
                    Console.WriteLine("Update an {0} item : ", tableName);
                    Console.Write(" Input item ID in table {0}: ", tableName);
                    try {
                        tempID = int.Parse(Console.ReadLine());
                    } catch (FormatException) {
                        Console.WriteLine("Chi nhap so nguyen !");
                        System.Threading.Thread.Sleep(3000);
                        tableMenu(tableName);
                    }
                    // ------------------ Create an instance of this the type -----------------
                    object updateShopItem = Activator.CreateInstance(Type.GetType("ShopManager." + tableName));

                    //-------------Get a list of shop's item properties ----------------
                    List<PropertyInfo> propList = updateShopItem.GetType().GetProperties().ToList();
                    foreach (PropertyInfo propType in propList) {
                        Console.Write("Give {0} value : ", propType.Name);

                        // -------------- Convert user input data ( String ) to correct type of property --------------
                        var valueForProp = Convert.ChangeType(Console.ReadLine(), propType.PropertyType);

                        // -------------- Set value from user to property -----------------
                        updateShopItem.GetType().GetProperty(propType.Name).SetValue(updateShopItem, valueForProp);
                    }
                    con.update(updateShopItem, tempID);
                    break;
                case 5:
                    return;
                default:
                    Console.WriteLine("Chi nhap cac so tu 1 den 5");
                    System.Threading.Thread.Sleep(5000);
                    tableMenu(tableName);
                    break;
            }
        }
    }
}
