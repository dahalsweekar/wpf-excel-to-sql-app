using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExcelToSQLTable;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;

namespace ExcelToSQLTable;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    string gfileName = string.Empty;
    List<List<string>> gdataRows = new List<List<string>>();
    List<string> gheaderRow = new List<string>();
    string gConnectionstring = string.Empty;
    string savedPassword = string.Empty;
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Browse_clk(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.DefaultExt = ".xlsx";
        dialog.Filter = "Excel Documents (*.xlsx)|*.xlsx";
        bool? result = dialog.ShowDialog();
        string filename = string.Empty;
        string connectionString = string.Empty;

        if (result == true)
        {
            filename = dialog.FileName;
            gfileName = dialog.FileName;
        }

        txtExcel.Text = filename;

        try
        {
            Excel pExcel = new Excel(filename);
            (gheaderRow, gdataRows) = pExcel.LoadExcelData();
            msgBox.Content = "Excel file parsed successfully.";
            msgBox.Foreground = new SolidColorBrush(Colors.Green);
        }
        catch (Exception ex)
        {
            msgBox.Content = ex.Message.ToString();
            msgBox.Foreground = new SolidColorBrush(Colors.Red);
            throw new Exception(ex.Message);
        }
    }

    private void On_TextChanged(object sender, EventArgs e)
    {
        if (savedPassword != txtpassword.Text)
        {
            savedPassword += txtpassword.Text[txtpassword.Text.Length - 1] == '*' ? "" : txtpassword.Text[txtpassword.Text.Length - 1];
        }
        if (chkShowPass.IsChecked == true)
        {
            txtpassword.Text = new string('*', txtpassword.Text.Length);
        }
        else
        {
            txtpassword.Text = savedPassword;
        }
    }

    private void Connect_clk(object sender, RoutedEventArgs e)
    {
        string server = txtsevername.Text;
        string database = txtdatabase.Text;
        string username = txtusername.Text;
        string password = savedPassword;

        if (!string.IsNullOrWhiteSpace(server) && !string.IsNullOrWhiteSpace(database)&& !string.IsNullOrWhiteSpace(username)&& !string.IsNullOrWhiteSpace(password))
        {
            string connectionString = $"Server={server};Database={database};User Id={username};Password={password};TrustServerCertificate=true;";

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.ConnectionStrings.ConnectionStrings.Remove("UserDB");
            config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings("UserDB", connectionString, "System.Data.SqlClient"));
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");

            DB db = new DB(connectionString);
            string res = db.LoadDatabase();

            if (res=="success") {
                msgBox.Content = "Connection successful";
                msgBox.Foreground = new SolidColorBrush(Colors.Green);
                btnCreate.Visibility = Visibility.Visible;
                stkCreate.Visibility = Visibility.Visible;
            }
            else {
                msgBox.Content = res;
                msgBox.Foreground = new SolidColorBrush(Colors.Red);
            }
            gConnectionstring = connectionString;
        }
        else
        {
            msgBox.Content = "Invalid Credentials for DB connection";
            msgBox.Foreground = new SolidColorBrush(Colors.Red);
        }
    }

    private void CreateTable_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(tableName.Text))
        {
            DB dB = new DB(gConnectionstring);
            string res = dB.ConvertToTable(gheaderRow, gdataRows, tableName.Text);
            msgBox.Content = res;
            msgBox.Foreground = new SolidColorBrush(Colors.Green);
        }
        else
        {
            msgBox.Content = "Please enter a table name";
            msgBox.Foreground = new SolidColorBrush(Colors.Red);
        }
    }

    private void CheckBox_CheckUnCheck(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtpassword.Text))
        {
            if (!txtpassword.Text.Contains('*'))
            {
                int len = txtpassword.Text.Length;
                string hiddenState = new string('*', len);
                txtpassword.Text = hiddenState;
            }
            else
            {
                txtpassword.Text = savedPassword;
            }
            
        }
    }
}