using System;
using System.Diagnostics;
using System.Windows;
using MySql.Data.MySqlClient;

namespace DatabaseAdmin {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e) {
            var myConnectionString = String.Format("server={0};uid={1};pwd={2};",
                DatabaseLocation.Text,
                DatabaseUser.Text,
                DatabasePassword.Text
                );

            try {
                var conn = new MySqlConnection();
                conn.ConnectionString = myConnectionString;
                Log("Starting clearning " + DatabaseName.Text);

                var querySetForeignFalse = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", conn);
                var querySetForeignTrue = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", conn);
                var queryDrop = new MySqlCommand("DROP DATABASE " + DatabaseName.Text, conn);
                var queryCreate = new MySqlCommand("CREATE DATABASE " + DatabaseName.Text, conn);

                conn.Open();

                querySetForeignFalse.ExecuteNonQuery();
                queryDrop.ExecuteNonQuery();
                queryCreate.ExecuteNonQuery();
                querySetForeignTrue.ExecuteNonQuery();

                Log("Finished clearning " + DatabaseName.Text);

                conn.Close();
            }
            catch (MySqlException ex) {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void SeedButton_Click(object sender, RoutedEventArgs e) {
            ExecutePhp("artisan db:seed");
        }

        private void MigrateSeedButton_Click(object sender, RoutedEventArgs e) {
            ExecutePhp("artisan migrate --seed");
        }

        private void MigrateRefreshSeedButton_Click(object sender, RoutedEventArgs e) {
            ExecutePhp("artisan migrate:refresh --seed");
        }

        private void MigrateRefreshButton_Click(object sender, RoutedEventArgs e) {
            ExecutePhp("artisan migrate:refresh");
        }

        private void MigrateButton_Click(object sender, RoutedEventArgs e) {
            ExecutePhp("artisan migrate");
        }

        private void ComposerDumpAutoloadButton_Click(object sender, RoutedEventArgs e) {
            ExecuteComposer("dumpautoload");
        }



        #region helpers

        private void Log(string newText) {
            if (string.IsNullOrEmpty(newText)) return;
            AddToLog("[INFO] " + newText);
        }

        private void Error(string newText) {
            if (string.IsNullOrEmpty(newText)) return;
            AddToLog("[ERROR] " + newText);
        }

        private void AddToLog(string newText) {
            
            Dispatcher.BeginInvoke(new Action(delegate {
                var message = "[" + DateTime.Now + "] " + newText;
                Logbox.Items.Add(message);
                Logbox.ScrollIntoView(Logbox.Items[Logbox.Items.Count-1]);
            }));
        }

        private void ExecutePhp(string command) {
            Log(command);
            var p = new Process {
                StartInfo = new ProcessStartInfo(PhpDir.Text, command) {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = WorkingDir.Text
                }
            };
            p.OutputDataReceived += (s, a) => {
                Log(a.Data);
                Console.WriteLine(a.Data);
            };
            p.ErrorDataReceived += (s, a) => { Error(a.Data); };
            p.Start();
            p.BeginOutputReadLine();
        }

        private void ExecuteComposer(string command) {
            command = "composer.phar " + command;
            Log(command);

            var p = new Process
            {
                StartInfo = new ProcessStartInfo(PhpDir.Text, command)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = WorkingDir.Text
                }
            };
            p.OutputDataReceived += (s, a) =>
            {
                Log(a.Data);
                Console.WriteLine(a.Data);
            };
            p.ErrorDataReceived += (s, a) => { Error(a.Data); };
            p.Start();
            p.BeginOutputReadLine();
        }


        #endregion
    }
}