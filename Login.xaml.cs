using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml;
using RfReader_demo.Helper;
using System.Data;
using System.Configuration;
using Sentry;
using System.Globalization;

namespace RfReader_demo
{
    public partial class Login : Window
    {
        public string conn = string.Empty;
        public Login()
        {
            conn = ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
            if (!string.IsNullOrEmpty(conn))
            {
                SentrySdk.Init(conn);
            }
            else { MessageBox.Show("Sentry Key not Found", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); }
            
            InitializeComponent();
            CreatingLoginCredentialFile();
            CreatingLogFile();
        }
        
        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Shutdown();
                MainWindow.InsertingLogTextToLogFile("Application close successfully." + " (" + DateTime.Now.ToString() + ")");

            }
            catch(Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }

        private void btn_Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lbl_Message.Content = string.Empty;
                var get_UserName = txtbx_UserName.Text;
                var get_Password = txtbx_Password.Password;

                if ((get_UserName.Length < 1) && (get_Password.Length < 1))
                {
                    lbl_Message.Content = "Please enter User Name & Password.";
                }
                else if (get_UserName.Length < 1)
                {
                    lbl_Message.Content = "Please enter User Name.";
                }
                else if (get_Password.Length < 1)
                {
                    lbl_Message.Content = "Please enter Password.";
                }
                else
                {
                    if (get_UserName == DefaultCredentials.username && get_Password == GetPasswordFromXML())
                    {
                        MainWindow mw = new MainWindow();
                        mw.Show();
                        this.Close();
                    }
                    else
                    {
                        lbl_Message.Content = "UserName or Password is incorrect";
                        MainWindow.InsertingLogTextToLogFile("UserName or Password is incorrect." + " (" + DateTime.Now.ToString() + ")");
                    }
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                MessageBox.Show(ex.Message);
            }
        }

        private void CreatingLoginCredentialFile()
        {
            string pathDir = AppDomain.CurrentDomain.BaseDirectory + "\\Credential";
            try
            {
                if (!Directory.Exists(pathDir))
                {
                    Directory.CreateDirectory(pathDir);
                }
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Credential\\LoginCredential.xml";
                
                if (!File.Exists(path))
                {
                    XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                    xmlWriterSettings.Indent = true;
                    xmlWriterSettings.NewLineOnAttributes = true;
                    using (XmlWriter xmlWriter = XmlWriter.Create(path, xmlWriterSettings))
                    {
                        xmlWriter.WriteStartDocument();
                        xmlWriter.WriteStartElement("LoginCredential");
                        xmlWriter.WriteElementString("UserName", Crypto.Encrypt(DefaultCredentials.username));
                        xmlWriter.WriteElementString("Password", Crypto.Encrypt(DefaultCredentials.Password));
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteEndDocument();
                        xmlWriter.Flush();
                        xmlWriter.Close();
                    }
                }
            }
            catch(Exception ex)
            {                
                SentrySdk.CaptureException(ex);
                MessageBox.Show(ex.Message);
            } 
        }

        private bool CheckDefaultPasswordExist()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Credential\\LoginCredential.xml";
            DataSet ds = new DataSet();
            bool Is_Validate = false;
            try
            {
                ds.ReadXml(path);
                var dt = ds.Tables["LoginCredential"];
                if (dt != null)
                {
                    var getPass_Decrypt = Crypto.Decrypt(dt.Rows[0][1].ToString());

                    if (getPass_Decrypt == "adminadmin")
                    {
                        Is_Validate  = true;
                    }
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                MessageBox.Show(ex.Message);
            }
            return Is_Validate;
        }
        private string GetPasswordFromXML()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Credential\\LoginCredential.xml";
            DataSet ds = new DataSet();
            string Message = string.Empty;
            try
            {
                ds.ReadXml(path);
                var dt = ds.Tables["LoginCredential"];
                if (dt != null)
                {
                    Message = Crypto.Decrypt(dt.Rows[0][1].ToString());
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureMessage(ex.Message);
                Message = ex.Message;
                MessageBox.Show(Message);
            }
            return Message;
        }
        public void CreatingLogFile()
        {
            try
            {
                var dt = DateTime.Now;
                var year = dt.Year;
                var month = dt.Month;
                var day = dt.Day;
                var hour = dt.Hour.ToString("00.##");
                var min = dt.Minute.ToString("00.##");
                var sec = dt.Second.ToString("00.##");
                var tt = dt.ToString("tt", CultureInfo.InvariantCulture);

                string combileAllTime = year + "" + month + "" + day + "-" + hour + "" + min + "" + sec;
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string fileName = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\Log-" + combileAllTime + ".txt";
                File.Create(fileName).Dispose();
            }
            catch (Exception ex)
            {                
                SentrySdk.CaptureException(ex);
                MessageBox.Show(ex.Message);
            }
        }

        public class DefaultCredentials
        {
            public static string username = "admin";
            public static string Password = "adminadmin";
        }
    }
}
