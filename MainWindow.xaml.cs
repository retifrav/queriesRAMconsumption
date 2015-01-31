using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace queriesRAMconsumption
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Таймер, событие которого делает всю магию
        /// </summary>
        private System.Timers.Timer timer2nextUpdate = new System.Timers.Timer();
        /// <summary>
        /// Интервал по умолчанию
        /// </summary>
        private int default_interval = 5;
        /// <summary>
        /// Сервер по умолчанию
        /// </summary>
        private string default_server = Properties.Settings.Default.defaultServer;
        /// <summary>
        /// Непотребный размер запрашиваемой оперативки
        /// </summary>
        private int limitMB = Properties.Settings.Default.limit4query;
        /// <summary>
        /// Счётчик тяжёлых запросов
        /// </summary>
        private int counter = 0;
        /// <summary>
        /// Самый тяжёлый запрос
        /// </summary>
        private int maxWeightVal = 0;
        /// <summary>
        /// Последовательность выполнения
        /// </summary>
        private bool isExecuting = false;
        /// <summary>
        /// SQL-запрос проверки
        /// </summary>
        private StringBuilder query = new StringBuilder();

        public MainWindow()
        {
            InitializeComponent();

            this.timer2nextUpdate.AutoReset = true;
            this.timer2nextUpdate.Elapsed
                += new System.Timers.ElapsedEventHandler(this.timer2nextUpdate_tick);

            interval.Text = default_interval.ToString();
            server.Text = default_server;

            query.Clear();
            query.Append("SELECT TOP 1 requested_memory_kb / 1024, DB_NAME(st.dbid), text ");
            query.Append("FROM sys.dm_exec_query_memory_grants AS mg ");
            query.Append("CROSS APPLY sys.dm_exec_sql_text(mg.sql_handle) AS st ");
            query.Append("ORDER BY requested_memory_kb DESC");

            // when F1 is pressed, show help window
            CommandBinding helpBinding = new CommandBinding(ApplicationCommands.Help);
            helpBinding.Executed += f1pressed;
            CommandBindings.Add(helpBinding);
        }

        private void f1pressed(object sender, ExecutedRoutedEventArgs e)
        {
            HelpWindow hlp = new HelpWindow();
            hlp.ShowDialog();
        }

        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            counter = 0;
            maxWeightVal = 0;
            heavyQueriesCnt.Content = "0";
            maxWeight.Content = "0 МБ";
            
            log.Clear();
            logHeavy.Clear();

            int interv = 0;
            try 
            {
                interv = int.Parse(interval.Text.Trim());
            }
            catch
            {
                MessageBox.Show(
                    "Timer must be integer number.",
                    "Timer setting error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
                return;
            }
            if (interv != 0)
            {
                if (string.IsNullOrEmpty(server.Text.Trim()))
                {
                    MessageBox.Show(
                        "Specify the name of SQL-server.",
                        "Server name is empty",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );
                    return;
                }

                default_server = server.Text.Trim();
                
                timer2nextUpdate.Interval = interv * 1000;
                this.timer2nextUpdate.Start();

                interval.IsEnabled = false;
                server.IsEnabled = false;
                btn_start.Visibility = System.Windows.Visibility.Hidden;
                btn_stop.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void timer2nextUpdate_tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (isExecuting)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    log.AppendText(string.Format("[{0}] - {1}{2}",
                      DateTime.Now.ToString(),
                      "timer event has been skipped",
                      Environment.NewLine
                      ));
                }
                    )); 
                return;
            }

            isExecuting = true;

            string val = string.Empty,
                   db = string.Empty,
                   txt = string.Empty;
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"[ ]{2,}", options);

            try
            {
                using (SqlConnection sqlConn =
                                    new SqlConnection(
                                        string.Format(
                                            "Data Source={0};Initial Catalog=master;Persist Security Info=True;User ID={1};Password={2}",
                                            default_server,
                                            Properties.Settings.Default.db_login,
                                            Properties.Settings.Default.db_password
                                            )
                                        ))
                using (SqlCommand cmd = new SqlCommand(query.ToString(), sqlConn))
                {
                    sqlConn.Open();
                    cmd.CommandTimeout = 3;

                    SqlDataReader reader;
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        val = reader[0].ToString();
                        db = reader[1].ToString();
                        try { txt = regex.Replace(reader[2].ToString().Replace("\t", @" "), @" ") + "..."; }
                        catch (Exception ex) { txt = "Couldn't get the query text. " + ex.Message; }
                        if (txt.Length > 900) { txt = txt.Substring(0, 900) + "..."; }
                    }

                    sqlConn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Couldn't execute the query. {0}.", ex.Message),
                    "Error getting value",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
                isExecuting = false;
                return;
            }

            if (!string.IsNullOrEmpty(val))
            {
                if (int.Parse(val) > limitMB)
                {
                    counter++;
                    int valInt = int.Parse(val);
                    if (valInt > maxWeightVal) { maxWeightVal = valInt; }
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            heavyQueriesCnt.Content = counter.ToString();
                            logHeavy.AppendText(string.Format("{3}{2}{2}[{0}] - {1}{2}{2}Database: {4}{2}Query text:{2}{2}{5}{2}{2}",
                                DateTime.Now.ToString(),
                                val + " MB",
                                Environment.NewLine,
                                "-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-",
                                string.IsNullOrEmpty(db) ? "unknown" : db,
                                txt
                                ));
                            logHeavy.ScrollToEnd();
                            maxWeight.Content = maxWeightVal.ToString() + " MB";
                        }
                        ));
                    val = val + " MB <--- heavy query";
                }
                else { val = val + " MB"; }

                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    { log.AppendText(string.Format("[{0}] - {1}{2}",
                        DateTime.Now.ToString(),
                        val,
                        Environment.NewLine
                        )); }
                    )); 
            }
            else
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    { log.AppendText(string.Format("[{0}] - {1}{2}",
                        DateTime.Now.ToString(),
                        "error getting data",
                        Environment.NewLine
                        ));
                    }
                    ));
            }
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                { log.ScrollToEnd(); }
                ));
            isExecuting = false;
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            timer2nextUpdate.Stop();

            interval.IsEnabled = true;
            server.IsEnabled = true;
            btn_start.Visibility = System.Windows.Visibility.Visible;
            btn_stop.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
