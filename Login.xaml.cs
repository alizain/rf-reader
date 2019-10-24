using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
        public string conn = ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
        public Login()
        {
            using (SentrySdk.Init(conn))
            {
                InitializeComponent();
                CreatingLoginCredentialFile();
                CreatingLogFile();
            }
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
                SentrySdk.CaptureMessage(ex.Message);
            }
        }

        private void btn_Login_Click(object sender, RoutedEventArgs e)
        {
            using (SentrySdk.Init(conn))
            {
                try
                {
                    //MainWindow mainw = new MainWindow();
                    //mainw.CreatingLogFile();
                    lbl_Message.Content = "";
                    var get_UserName = txtbx_UserName.Text;
                    var get_Password = txtbx_Password.Password;

                    if ((get_UserName.Length < 1 || get_UserName == null) && (get_Password.Length < 1 || get_Password == null))
                    {
                        lbl_Message.Content = "Please enter User Name & Password.";
                        MainWindow.InsertingLogTextToLogFile("Please enter User Name & Password." + " (" + DateTime.Now.ToString() + ")");
                        SentrySdk.CaptureMessage("Please enter User Name & Password.");
                    }
                    else if (get_UserName.Length < 1 || get_UserName == null)
                    {
                        lbl_Message.Content = "Please enter User Name.";
                        MainWindow.InsertingLogTextToLogFile("Please enter User Name." + " (" + DateTime.Now.ToString() + ")");
                        SentrySdk.CaptureMessage("Please enter User Name.");
                    }
                    else if (get_Password.Length < 1 || get_Password == null)
                    {
                        lbl_Message.Content = "Please enter Password.";
                        MainWindow.InsertingLogTextToLogFile("Please enter Password." + " (" + DateTime.Now.ToString() + ")");
                        SentrySdk.CaptureMessage("Please enter Password.");
                    }
                    else
                    {
                        var getPassword_FromXML = GetPasswordFromXML();

                        if (get_UserName == "admin" && get_Password == getPassword_FromXML)
                        {
                            MainWindow mw = new MainWindow();
                            mw.Show();
                            this.Close();
                        }
                        else if ((get_UserName.Length > 0) && (get_Password.Length > 0))
                        {
                            lbl_Message.Content = "Password is incorrect. Please try again.";
                            MainWindow.InsertingLogTextToLogFile("Password is incorrect. Please try again." + " (" + DateTime.Now.ToString() + ")");
                            SentrySdk.CaptureMessage("Password is incorrect. Please try again.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureMessage(ex.Message);
                }
            }
        }

        private void CreatingLoginCredentialFile()
        {
            var username = "admin";
            var password = "adminadmin";

            //string pathDir = System.IO.Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName + "\\Credential");
            string pathDir = AppDomain.CurrentDomain.BaseDirectory + "\\Credential";
            if (!Directory.Exists(pathDir))
            {
                Directory.CreateDirectory(pathDir);
            }

            //string path = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName;
            //string path = System.IO.Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName + "\\Credential", "LoginCredential.xml");
            //string fileName = System.IO.Path.Combine(path, "LoginCredential.xml");
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Credential\\LoginCredential.xml";

            try
            {
                var Status_DefaultPasswordExist = CheckDefaultPasswordExist();

                if (File.Exists(path))
                {
                    if (Status_DefaultPasswordExist == true)
                    { }
                }
                else
                {
                    XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                    xmlWriterSettings.Indent = true;
                    xmlWriterSettings.NewLineOnAttributes = true;
                    using (XmlWriter xmlWriter = XmlWriter.Create(path, xmlWriterSettings))
                    {
                        xmlWriter.WriteStartDocument();
                        /*LoginCredential Config*/
                        xmlWriter.WriteStartElement("LoginCredential");
                        xmlWriter.WriteElementString("UserName", Helper.Crypto.Encrypt(username));
                        xmlWriter.WriteElementString("Password", Helper.Crypto.Encrypt(password));
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteEndDocument();
                        xmlWriter.Flush();
                        xmlWriter.Close();
                    }
                }
            }
            catch(Exception ex)
            {                
                SentrySdk.CaptureMessage(ex.Message);
            } 
        }

        private bool CheckDefaultPasswordExist()
        {
            //string path = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName;
            //string path = System.IO.Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName + "\\Credential", "LoginCredential.xml");
            //string fileName = System.IO.Path.Combine(path, "LoginCredential.xml");
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Credential\\LoginCredential.xml";

            DataSet ds = new DataSet();
            try
            {
                ds.ReadXml(path);

                var dt = ds.Tables["LoginCredential"];
                if (dt != null)
                {
                    var getUserName_XML = dt.Rows[0][0].ToString();
                    var getPass_XML = dt.Rows[0][1].ToString();
                    var getUserName_Decrypt = Helper.Crypto.Decrypt(getUserName_XML);
                    var getPass_Decrypt = Helper.Crypto.Decrypt(getPass_XML);

                    if (getPass_Decrypt == "adminadmin")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureMessage(ex.Message);
                return false;
            }
            return false;
        }
        private string GetPasswordFromXML()
        {
            //string path = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName;
            //string path = System.IO.Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName + "\\Credential", "LoginCredential.xml");
            //string fileName = System.IO.Path.Combine(path, "LoginCredential.xml");
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Credential\\LoginCredential.xml";

            DataSet ds = new DataSet();
            try
            {
                ds.ReadXml(path);

                var dt = ds.Tables["LoginCredential"];
                if (dt != null)
                {
                    var getUserName_XML = dt.Rows[0][0].ToString();
                    var getPass_XML = dt.Rows[0][1].ToString();
                    var getUserName_Decrypt = Helper.Crypto.Decrypt(getUserName_XML);
                    var getPass_Decrypt = Helper.Crypto.Decrypt(getPass_XML);

                    return getPass_Decrypt;
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureMessage(ex.Message);
                return null;
            }
            return null;
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

                //string path = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName + "\\Logs";
                //string path = System.IO.Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName + "\\Logs");
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                //string fileName = System.IO.Path.Combine(path, "Log-" + combileAllTime + ".txt");
                string fileName = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\Log-" + combileAllTime + ".txt";
                File.Create(fileName).Dispose();
            }
            catch (Exception ex)
            {                
                MainWindow.InsertingLogTextToLogFile(ex.Message.ToString());
                SentrySdk.CaptureException(ex);
            }
        }
    }
}
