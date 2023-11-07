using static System.Console;
using System.Data.Odbc;
using System.Configuration;
using System.Globalization;//this is for case-insensitive string comparison
using System.Collections.ObjectModel;//this is for ObservableCollection
using System.Diagnostics.Metrics;
using System.Drawing;


namespace COMP609_A2_Console_App_Kadin_Ethan
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var app = new App();

            app.PrintConsole();
        }

    }
    internal class App
    {
        public List<Animal> Companies { get; set; }
        OdbcConnection Conn;
        public App()
        {
            this.Conn = Util.GetConn();
            Companies = new List<Animal>();
            ReadDB();
        }
        public void PrintConsole()
        {
            Companies.ToList().ForEach(WriteLine);
        }

        internal int CommitDB(OdbcCommand cmd)
        {
            try
            {
                cmd.Transaction = Conn.BeginTransaction();
                int numRowsAffected = cmd.ExecuteNonQuery();
                cmd.Transaction.Commit();
                return numRowsAffected;
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message);
                //something went wrong while commiting changes to db
                //let's rollback the changes
                if (cmd.Transaction != null)
                    cmd.Transaction.Rollback();
                return 0;//zero row affected
            }
        }
        internal void ReadDB()
        {
            using (var cmd = Conn.CreateCommand())
            {
                cmd.Connection = Conn;

                string sql;
                OdbcDataReader reader;

                sql = "select * from Cow";
                cmd.CommandText = sql;
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = Util.GetInt(reader["ID"]);
                    double water = Util.GetDouble(reader["Water"]);
                    int cost = Util.GetInt(reader["Cost"]);
                    double weight = Util.GetDouble(reader["Weight"]);
                    string colour = Util.GetString(reader["Colour"]);
                    double milk = Util.GetDouble(reader["Milk"]);
                    if (id == Util.BAD_INT ||
                        water == Util.BAD_DOUBLE ||
                        cost == Util.BAD_INT ||
                        weight == Util.BAD_DOUBLE ||
                        colour == Util.BAD_STRING ||
                        milk == Util.BAD_DOUBLE)
                    {
                        WriteLine("bad row detected");
                        continue;//corrupted row, skip
                    }
                    var man = new Cow(id, water, cost, weight, colour, milk);
                    Companies.Add(man);
                }
                reader.Close();
            }
        }

        public int ConsoleGetID()
        {
            int id;
            while (true)
            {
                Write("Enter id: ");
                string? s = ReadLine();
                if (int.TryParse(s, out id))
                    break;
                else
                    WriteLine("Invalid");
            }
            return id;
        }
        public void ConsoleQueryByID()
        {
            WriteLine("===Query Record===");
            // read id via console
            int id = ConsoleGetID();
            var c = GetObjectByID(id);
            // display info in console
            if (c == null)
                WriteLine($"Non-existent id: {id}");
            else
                WriteLine(c);
        }
        public Animal? GetObjectByID(int id)
        {
            // retrieve object info using id
            return Companies.FirstOrDefault(x => x.ID == id);
        }
    }
    internal class Cow : Animal
    {
        public double Milk { get; set; }
        public Cow(int ID, double water, int cost, double weight, string colour, double milk) : base(ID, water, cost, weight, colour, milk)
        {
            this.Milk = milk;
        }
        public override string ToString()
        {
            return $"{this.GetType().Name,-15}{ID,-5}{Water,-10}{Cost,-10}{Weight,-10}{Colour,-10}{Milk,-10}";
        }
    }

    internal abstract class Animal
    {
        public int ID { get; set; }
        public double Water { get; set; }
        public int Cost { get; set; }
        public double Weight { get; set; }
        public string Colour { get; set; }
        public double Extra { get; set; }
        public Animal(int ID, double water, int cost, double weight, string colour, double extra)
        {
            this.ID = ID;
            this.Water = water;
            this.Cost = cost;
            this.Weight = weight;
            this.Colour = colour;
            this.Extra = extra;
        }

    }

    internal static class Util
    {
        internal static readonly string BAD_STRING = string.Empty;
        internal static readonly int BAD_INT = Int32.MinValue;
        internal static readonly double BAD_DOUBLE = Double.MinValue;
        internal static int GetInt(object o)
        {
            if (o == null) return BAD_INT;
            int n;
            if (int.TryParse(o.ToString(), out n) == false)
                return BAD_INT;
            return n;
        }
        internal static double GetDouble(object o)
        {
            if (o == null) return BAD_DOUBLE;
            double n;
            if (double.TryParse(o.ToString(), out n) == false)
                return BAD_DOUBLE;
            return n;
        }
        internal static string GetString(object o)
        {

            //?? null-coalescing operator, it returns the operand on its left if it's not null; otherwise, the operand on its right is return;
            return o?.ToString() ?? BAD_STRING;
        }
        internal static OdbcConnection GetConn()
        {
            string? dbstr = ConfigurationManager.AppSettings.Get("odbcString");
            string fpath = @"C:\Users\kadin\Downloads\FarmData.accdb";
            string connstr = dbstr + fpath;
            var conn = new OdbcConnection(connstr);
            conn.Open();
            return conn;
        }
    }
}