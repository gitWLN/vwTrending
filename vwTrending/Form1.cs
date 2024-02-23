using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vwTrending
{
    public partial class Form1 : Form
    {

        private string query = "SELECT monster FROM vwTrending_LW";
        private bool isEdit = false;
        private Dictionary<string, dynamic> config;
        public Form1()
        {
            InitializeComponent();

            //Load Config
            this.textBox1.Text = $"Loading Config...{Environment.NewLine}";
            try
            {
                config = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                 File.ReadAllText("appsettings.json"));
            }
            catch (Exception ex){
                MessageBox.Show($"Cannot read appsettings.json{Environment.NewLine}{ex.ToString()}");
            }
            this.textBox1.AppendText($"Config Loaded!{Environment.NewLine}");
            

            this.textBox2.AppendText($"{config["CsvConfig"]["FilePath"]}");
            query = $"{config["DatabaseConfig"]["Query"]}";

            this.textBox1.AppendText($"Ready...{Environment.NewLine}");
        }

      

        private void button1_Click(object sender, EventArgs e)
        {
            this.textBox1.Clear();
            if (MessageBox.Show("Run", "Are you Sure?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                return;
       

            var connectionString = $"Server={config["DatabaseConfig"]["Server"]};Database={config["DatabaseConfig"]["Database"]};Integrated Security=True;";


            // Connect to SQL Server using Windows Authentication
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    this.textBox1.AppendText($"Connected to database. {Environment.NewLine}");

                    // Execute SQL query
                    var monsters = connection.Query<string>(query);

                    // Check if the query returned any monsters
                    if (monsters != null && monsters.Any())
                    {                        
                        // Write results to CSV
                        using (var writer = new StreamWriter(this.textBox2.Text))
                        {
                            int reportNumber = (int)this.numericUpDown1.Value;
                            int counter = 0;
                            foreach (var monster in monsters)
                            {
                                writer.WriteLine(monster);
                                counter++;
                                if (counter % reportNumber == 0) // Update progress every x records
                                {
                                    if ((this.textBox1.Text.Length + 500) > this.textBox1.MaxLength) 
                                        this.textBox1.Clear();

                                    this.textBox1.AppendText($"{counter} records processed...{Environment.NewLine}");
                                }
                            }
                        }
                        this.textBox1.AppendText($"Data export completed successfully. {Environment.NewLine}");
                    }
                    else
                    {
                        this.textBox1.AppendText($"No data found. {Environment.NewLine}");
                    }
                }
                catch (Exception ex)
                {
                    this.textBox1.AppendText($"An error occurred: {ex.Message}{Environment.NewLine}");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Load configuration
            var config = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                File.ReadAllText("appsettings.json"));

            var connectionString = $"Server={config["DatabaseConfig"]["Server"]};Database={config["DatabaseConfig"]["Database"]};Integrated Security=True;";
            this.textBox1.AppendText($"ConnectionString: {connectionString}{Environment.NewLine}");
            this.textBox1.AppendText($"Trying to connect...{Environment.NewLine}");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                        connection.Open();
                        this.textBox1.AppendText($"Connected is OK.{Environment.NewLine}");
                }
            }
            catch (Exception ex)
            {
                this.textBox1.AppendText($"Connected error.{Environment.NewLine}");
                this.textBox1.AppendText(ex.ToString() + Environment.NewLine);
            }
           

            this.textBox1.AppendText($"Connection Closed {Environment.NewLine}");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(!isEdit)
            {
                this.textBox1.Clear();
                this.textBox1.Text = query;
                this.button3.Text = "Save Query";
            }
            else
            {
                this.button3.Text = "Edit Query";
                query = this.textBox1.Text;
                this.textBox1.Clear();
                this.textBox1.Text = "Query saved in memory..";
            }
            isEdit = !isEdit;
        }
    }
}
