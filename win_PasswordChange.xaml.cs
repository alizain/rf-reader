using Sentry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;

namespace RfReader_demo
{
    public partial class win_PasswordChange : Window
    {
        public win_PasswordChange()
        {            
            try
            {
                string conn = ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
                if (!string.IsNullOrEmpty(conn))
                {
                    SentrySdk.Init(conn);
                }
                else { MessageBox.Show("Sentry Key not Found", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); }
                InitializeComponent();
                lbl_StatusMessage.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }
        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lbl_StatusMessage.Content = string.Empty;
                lbl_StatusMessage.Visibility = Visibility.Hidden;
                var oldPassword = txtbx_OldPassword.Password;
                var newPassword = txtbx_NewPassword.Password;
                var confirmPassword = txtbx_ConfirmPassword.Password;

                if (oldPassword.Length < 1 || newPassword.Length < 1 || confirmPassword.Length < 1)
                {
                    string msg = "Please fill out the fields.";
                    lbl_StatusMessage.Content = msg;
                    lbl_StatusMessage.Visibility = Visibility.Visible;
                }
                else
                {
                    if (CheckOldPassword(oldPassword))
                    {
                        if (newPassword == confirmPassword)
                        {
                            if (PasswordChanged(newPassword))
                            {
                                string msg = "Password changed successfully.";
                                MessageBox.Show("Password changed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                lbl_StatusMessage.Content = msg;
                                lbl_StatusMessage.Visibility = Visibility.Visible;
                                lbl_StatusMessage.Background = new SolidColorBrush(Colors.LightGreen);
                                lbl_StatusMessage.Foreground = new SolidColorBrush(Colors.Green);
                                lbl_StatusMessage.BorderBrush = new SolidColorBrush(Colors.Green);
                                lbl_StatusMessage.BorderThickness = new Thickness(2.0);
                                MainWindow.InsertingLogTextToLogFile(msg + " (" + DateTime.Now.ToString() + ")");
                                SentrySdk.CaptureMessage(msg);
                                this.Close();
                            }
                        }
                        else
                        {
                            MessageBox.Show("New Password and Confirm Password is not same.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            string msg = "New Password and Confirm Password is not same.";
                            lbl_StatusMessage.Content = msg;
                            lbl_StatusMessage.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }
        
        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }
        private void btn_Quit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }
        private bool CheckOldPassword(string pass)
        {
            bool ValidPassword = false;
            try
            {
                string fileName = AppDomain.CurrentDomain.BaseDirectory + "\\Credential\\LoginCredential.xml";
                DataSet ds = new DataSet();
                ds.ReadXml(fileName);
                var dt = ds.Tables["LoginCredential"];
                if (dt != null)
                {
                    if (pass == Helper.Crypto.Decrypt(dt.Rows[0][1].ToString()))
                    {
                        ValidPassword = true;
                    }
                    else
                    {
                        MessageBox.Show("Old Password not matched.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        string msg = "Old Password not matched.";
                        lbl_StatusMessage.Content = msg;
                        lbl_StatusMessage.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                MainWindow.InsertingLogTextToLogFile(ex.Message + " (" + DateTime.Now.ToString() + ")");
                SentrySdk.CaptureException(ex);
                MessageBox.Show(ex.Message);
            }
            return ValidPassword;
        }
        private bool PasswordChanged(string newPassword)
        {
            bool _Changed = true;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + "\\Credential\\LoginCredential.xml");
                XmlNodeList _Password = doc.GetElementsByTagName("Password");
                _Password[0].InnerText = Helper.Crypto.Encrypt(newPassword);
                doc.Save(AppDomain.CurrentDomain.BaseDirectory + "\\Credential\\LoginCredential.xml");
            }
            catch (Exception ex)
            {
                MainWindow.InsertingLogTextToLogFile(ex.Message + " (" + DateTime.Now.ToString() + ")");
                SentrySdk.CaptureException(ex);
                MessageBox.Show(ex.Message);
                _Changed = false;
            }
            return _Changed;
        }
    }
}
