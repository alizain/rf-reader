using Newtonsoft.Json;
using RfReader_demo.BAL;
using RfReader_demo.DAL;
using RfReader_demo.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using static RfReader_demo.BAL._BAL;
using Sentry;
using System.Configuration;

namespace RfReader_demo
{
    public partial class MainWindow : Window
    {
        #region _Variables
        public static string ConnectionString;
        private bool IsLive = false;
        private DataTable allPortData = new DataTable();        
        private string EditValue;
        SerialPort[] sps = null;
        _BAL Blayer = new _BAL();
        vt_Common vtCommon = new vt_Common();
        public static string conn = string.Empty;
        #endregion

        public MainWindow()
        {
            conn = ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
            if (!string.IsNullOrEmpty(conn))
            {
                SentrySdk.Init(conn);
            }
            else { MessageBox.Show("Sentry Key not Found", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); }
            InitializeComponent();
            readXML();
            BindComboBoxWithAvailablePorts();
            InitializeAllPortsToSerialPorts(true);            
        }

        #region PORT FUNCTIONS (INITIALIZEALLPORTSTOSERIALPORTS, REGISTERPORTHANDLER, CHECKPORTSTATUS, CLOSEALLSERIALPORTS, DATARECIEVEDHANDLER)
        public void InitializeAllPortsToSerialPorts(bool starting)
        {
            var portName = string.Empty;
            if (starting)
            {
                RegisterPort_Handler();
            }
            else
            {
                CloseAllSerialPorts();
                RegisterPort_Handler();
                BindComboBoxWithAvailablePorts();
            }
        }
        public void RegisterPort_Handler()
        {
            string portName = string.Empty;
            if (allPortData != null && allPortData.AsEnumerable().ToList().Count > 0)
            {
                sps = new SerialPort[allPortData.AsEnumerable().ToList().Count];
                for (int i = 0; i < allPortData.AsEnumerable().ToList().Count; i++)
                {
                    try
                    {
                        portName = allPortData.Rows[i]["DevicePort"].ToString();
                        sps[i] = new SerialPort();
                        sps[i].PortName = portName;
                        sps[i].BaudRate = 9600;
                        sps[i].Parity = Parity.None;
                        sps[i].StopBits = StopBits.One;
                        sps[i].DataBits = 8;
                        sps[i].Handshake = Handshake.None;
                        sps[i].DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                        CheckPortsStatus(sps[i], false);

                    }
                    catch (Exception ex)
                    {
                        txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message.ToString())));
                        InsertingLogTextToLogFile(ex.Message.ToString());
                        SentrySdk.CaptureException(ex);
                    }
                }
            }
        }
        public void CheckPortsStatus(SerialPort sPort, bool Close)
        {
            try
            {
                if (Close)
                {
                    if (sPort.IsOpen)
                    {
                        sPort.Close();
                    }
                }
                else
                {
                    sPort.Open();
                }
            }
            catch (Exception ex)
            {                
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message.ToString())));
                InsertingLogTextToLogFile(ex.Message.ToString());
                SentrySdk.CaptureException(ex);
            }           
        }
        private void CloseAllSerialPorts()
        {
            if (sps != null)
            {
                for (int i = 0; i < sps.Length; i++)
                {
                    try
                    {
                        CheckPortsStatus(sps[i], true);
                    }
                    catch (Exception ex)
                    {
                        txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message.ToString())));
                        InsertingLogTextToLogFile(ex.Message.ToString());
                        SentrySdk.CaptureException(ex);
                    }
                }
            }
        }
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            try
            {                
                string data = sp.ReadLine();
                var checkPortName = sp.PortName;
                var rows = DecryptDeviceTable(vtCommon.dsXML.Tables["Devices"]).AsEnumerable().Where(r => r.Field<string>("DevicePort") == checkPortName).ToList();
                if (rows.Count > 0)
                {
                    string tablename = rows[0].ItemArray[0].ToString();
                    string[] arr = data.Split('\r');
                    string newVal = arr[0].Substring(arr[0].Length - 8);

                    int decValue = Convert.ToInt32(newVal, 16);
                    string newValueGet = Convert.ToString(decValue);
                    this.Dispatcher.Invoke(() =>
                    {
                        UpdateScreen(newValueGet, false, tablename, checkPortName);
                    });
                }
                else
                {
                    string Message = "Registered port not found in saved XML please restart application.";
                    txtLogs.Document.Blocks.Add(new Paragraph(new Run(Message)));
                    InsertingLogTextToLogFile(Message);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.ToString().Contains("thread exit"))
                { sp.Dispose();}
                SentrySdk.CaptureException(ex);
            }
        }
        #endregion

        #region XML FUNCTIONS (READ, COMPARE, SAVE)              
        void readXML()
        {
            string fileName = AppDomain.CurrentDomain.BaseDirectory + "\\Credential\\appData.xml";
            try
            {
                if (File.Exists(fileName))
                {
                    vtCommon.dsXML = new DataSet();
                    vtCommon.dsXML.ReadXml(fileName);                    
                    if (vtCommon.dsXML != null && vtCommon.dsXML.Tables.Count > 0 && vtCommon.dsXML.Tables[0].Rows.Count > 0)
                    {
                        var _db = vtCommon.dsXML.Tables["DatabaseConfig"];
                        var _devices = allPortData = vtCommon.dsXML.Tables["Devices"].Copy();                        

                        if (_db != null)
                        {
                            txt_IPAddress.Text = string.IsNullOrEmpty(_db.Rows[0][0].ToString()) ? string.Empty : Crypto.Decrypt(_db.Rows[0][0].ToString());
                            txt_PortNumber.Text = string.IsNullOrEmpty(_db.Rows[0][1].ToString()) ? string.Empty : Crypto.Decrypt(_db.Rows[0][1].ToString());
                            txt_ServerName.Text = string.IsNullOrEmpty(_db.Rows[0][2].ToString()) ? string.Empty : Crypto.Decrypt(_db.Rows[0][2].ToString());
                            txt_DatabaseName.Text = string.IsNullOrEmpty(_db.Rows[0][3].ToString()) ? string.Empty : Crypto.Decrypt(_db.Rows[0][3].ToString());
                            txt_Username.Text = string.IsNullOrEmpty(_db.Rows[0][4].ToString()) ? string.Empty : Crypto.Decrypt(_db.Rows[0][4].ToString());
                            txt_Password.Password = string.IsNullOrEmpty(_db.Rows[0][5].ToString()) ? string.Empty : Crypto.Decrypt(_db.Rows[0][5].ToString());
                        }
                        if (_devices != null)
                        {
                            _devices = allPortData = DecryptDeviceTable(vtCommon.dsXML.Tables["Devices"]);                                                     
                            dg_Devices.ItemsSource = null;
                            dg_Devices.CanUserAddRows = false;
                            dg_Devices.ItemsSource = _devices.DefaultView;
                        }
                        ConnectionString = "Data Source=(DESCRIPTION =" + "(ADDRESS = (PROTOCOL = TCP)(HOST = " + txt_IPAddress.Text + ")(PORT = " + txt_PortNumber.Text + "))" +
                            "(CONNECT_DATA =" + "(SERVER = DEDICATED)" + "(SERVICE_NAME = " + txt_ServerName.Text + ")));" + "User Id=" + txt_Username.Text + ";Password=" + txt_Password.Password + ";";
                    }
                }               
            }
            catch (Exception ex)
            {
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message.ToString())));
                InsertingLogTextToLogFile(ex.Message.ToString());
                SentrySdk.CaptureException(ex);
                MessageBox.Show(ex.Message);
            }
        }
        void CompareXML()
        {
            string fileName = AppDomain.CurrentDomain.BaseDirectory + "\\Credential\\appData.xml";
            try
            {
                vtCommon.dsNew.Clear();
                vtCommon.dsNew.ReadXml(fileName);               
                if (vtCommon.dsNew != null && vtCommon.dsNew.Tables.Count > 0 && vtCommon.dsNew.Tables[0].Rows.Count > 0)
                {
                    var _db = vtCommon.dsNew.Tables["DatabaseConfig"];
                    var _devices = allPortData = vtCommon.dsNew.Tables["Devices"];
                    if (_db != null)
                    {
                        txt_IPAddress.Text = Crypto.Decrypt(_db.Rows[0][0].ToString());
                        txt_PortNumber.Text = Crypto.Decrypt(_db.Rows[0][1].ToString());
                        txt_ServerName.Text = Crypto.Decrypt(_db.Rows[0][2].ToString());
                        txt_DatabaseName.Text = Crypto.Decrypt(_db.Rows[0][3].ToString());
                        txt_Username.Text = Crypto.Decrypt(_db.Rows[0][4].ToString());
                        txt_Password.Password = Crypto.Decrypt(_db.Rows[0][5].ToString());
                    }
                    if (_devices != null)
                    {
                        var new_Devicetbl = _devices.Copy();
                        new_Devicetbl = DecryptDeviceTable(vtCommon.dsNew.Tables["Devices"]);                        
                        dg_Devices.ItemsSource = null;                        
                        dg_Devices.CanUserAddRows = false;
                        dg_Devices.ItemsSource = new_Devicetbl.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message.ToString())));
                InsertingLogTextToLogFile(ex.Message.ToString());
                SentrySdk.CaptureException(ex);
            }
        }
        void SaveXML(string ipAddress, string portNumber, string serviceName, string databaseName, string username, string password, List<Devices> deviceData)
        {            
            string fileName = AppDomain.CurrentDomain.BaseDirectory + "\\Credential\\appData.xml";
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                xmlWriterSettings.NewLineOnAttributes = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(fileName, xmlWriterSettings))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("Databases");
                    xmlWriter.WriteStartElement("DatabaseConfig");
                    xmlWriter.WriteElementString("IPAddress", ipAddress);
                    xmlWriter.WriteElementString("PortNumber", portNumber);
                    xmlWriter.WriteElementString("ServiceName", serviceName);
                    xmlWriter.WriteElementString("DatabaseName", databaseName);
                    xmlWriter.WriteElementString("Username", username);
                    xmlWriter.WriteElementString("Password", password);
                    xmlWriter.WriteEndElement();                                        
                    foreach (var item in deviceData)
                    {
                        if (item.DeviceName != null || item.DevicePort != null)
                        {
                            item.DeviceName = Crypto.Encrypt(item.DeviceName);
                            item.DevicePort = Crypto.Encrypt(item.DevicePort);
                            xmlWriter.WriteStartElement("Devices");
                            xmlWriter.WriteElementString("DeviceName", item.DeviceName);
                            xmlWriter.WriteElementString("DevicePort", item.DevicePort);
                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            catch(Exception ex)
            {
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message.ToString())));
                InsertingLogTextToLogFile(ex.Message.ToString());
                SentrySdk.CaptureException(ex);
            }          
        }
        #endregion        

        #region INSERTINGTEXTTOLOGFILE        
        public static void InsertingLogTextToLogFile(string text)
        {
            RichTextBox txtLogs = new RichTextBox();
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
                DirectoryInfo di = new DirectoryInfo(path);
                FileInfo[] TXTFiles = di.GetFiles("*.txt");
                var latest_FileName = TXTFiles[TXTFiles.Count() - 1].Name.ToString();
                string pathcombine = System.IO.Path.Combine(path, latest_FileName);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(text);
                File.AppendAllText(pathcombine, sb.ToString());
                sb.Clear();
            }
            catch(Exception ex)
            {
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message.ToString())));               
                SentrySdk.CaptureException(ex);
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region BUTTONS HEADER (LIVE, TEST CONNECTION, OPEN CONFIG, OPEN LOG) (CLICK EVENTS)        
        private void btnLive_Click(object sender, RoutedEventArgs e)
        {
            var LiveBtn_TextBlockText = txtBlock_BtnLive.Text.ToString();

            if (LiveBtn_TextBlockText == "Go Live")
            {
                if (ConnectionTest(false))
                {
                    IsLive = true;
                    txtBlock_BtnLive.Text = "Go Offline";
                    btnLive.Background = Brushes.Red;
                    string text = "Live is Started ...! (" + DateTime.Now + ")";
                    txtLogs.Document.Blocks.Add(new Paragraph(new Run(text)));
                    InsertingLogTextToLogFile(text);
                }
                else
                {
                    string msg = "Database connection failed. Please make valid database configuration then Go Live.";
                    MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtLogs.Document.Blocks.Add(new Paragraph(new Run(msg + " (" + DateTime.Now.ToString() + ")")));
                    InsertingLogTextToLogFile(msg + " (" + DateTime.Now.ToString() + ")");
                    SentrySdk.CaptureMessage(msg);
                }
            }
            else
            {
                IsLive = false;
                txtBlock_BtnLive.Text = "Go Live";
                btnLive.Background = Brushes.LimeGreen;
                string text = "Live is Stopped ...! (" + DateTime.Now + ")";
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(text)));
                InsertingLogTextToLogFile(text);
            }
        }
        private void btn_TestConnection_Click(object sender, RoutedEventArgs e)
        {
            ConnectionTest(true);
        }
        private void btn_DbConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                string pathDir = AppDomain.CurrentDomain.BaseDirectory + "\\Credential\\appData.xml";
                if (File.Exists(pathDir))
                {
                    System.Diagnostics.Process.Start("notepad.exe", pathDir);
                }
                else
                {
                    string msg = "No Database Configuration file found.";
                    MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtLogs.Document.Blocks.Add(new Paragraph(new Run(msg + " (" + DateTime.Now.ToString() + ")")));
                    InsertingLogTextToLogFile(msg + " (" + DateTime.Now.ToString() + ")");
                    SentrySdk.CaptureMessage(msg);
                }
            }
            catch (Exception ex)
            {
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message + " (" + DateTime.Now.ToString() + ")")));
                InsertingLogTextToLogFile(ex.Message + " (" + DateTime.Now.ToString() + ")");
                SentrySdk.CaptureException(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btn_OpenLogFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                string pathDir = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
                System.Diagnostics.Process.Start(pathDir);
            }
            catch (Exception ex)
            {
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message + " (" + DateTime.Now.ToString() + ")")));
                InsertingLogTextToLogFile(ex.Message + " (" + DateTime.Now.ToString() + ")");
                SentrySdk.CaptureException(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region RICHTEXTBOX CHANGED EVENT
        private void txtLogs_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                txtLogs.ScrollToEnd();
                txtLogs.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
            catch (Exception ex)
            {
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message + " (" + DateTime.Now.ToString() + ")")));
                InsertingLogTextToLogFile(ex.Message + " (" + DateTime.Now.ToString() + ")");
                SentrySdk.CaptureException(ex);
            }
        }
        #endregion

        #region GRID BUTTONS (ADD NEW RECORD, EDIT, DELETE, CANCELEDIT) (CLICK EVENTS)       
        private void btn_AddNewRow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsLive)
                {
                    MessageBox.Show("Changes not allowed while application \"Is Live\".", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    string getPort_FromComboBox = comboBox.SelectedItem == null ? String.Empty : comboBox.SelectedItem.ToString(); // <-- Exception            
                    if (txt_DeviceName.Text.Length < 1 || getPort_FromComboBox.Length < 1)
                    {
                        string msg = "Please Enter Table Name or Device Port.";
                        MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        var getButtonText = btn_AddNewRow.Content.ToString();
                        if (getButtonText == "Update")
                        {
                            DataRow SelectRow = allPortData.Select("DevicePort='" + EditValue + "'").FirstOrDefault();
                            SelectRow["DeviceName"] = txt_DeviceName.Text;
                            SelectRow["DevicePort"] = comboBox.SelectedItem.ToString();
                            allPortData.AcceptChanges();
                            BindGridData(true);
                            btn_AddNewRow.Content = "Add";
                        }
                        else
                        {
                            BindGridData(false);
                        }
                    }
                    BindComboBoxWithAvailablePorts();
                }
            }
            catch (Exception ex)
            {
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message + " (" + DateTime.Now.ToString() + ")")));
                InsertingLogTextToLogFile(ex.Message + " (" + DateTime.Now.ToString() + ")");
                SentrySdk.CaptureException(ex);
                MessageBox.Show(ex.Message);
            }

        }        
        private void btnEdit_DataGrid_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BindComboBoxWithAvailablePorts();
                var selectedItem = dg_Devices.SelectedItem;
                if (selectedItem != null)
                {                    
                    var get_DeviceName = ((DataRowView)selectedItem).Row.ItemArray[0].ToString();
                    var get_DevicePort = ((DataRowView)selectedItem).Row.ItemArray[1].ToString();

                    EditValue = get_DevicePort.ToString();

                    txt_DeviceName.Text = get_DeviceName;
                    txt_DevicePort.Text = get_DevicePort;
                    ComboBoxItem item = new ComboBoxItem();
                    comboBox.Items.Add(get_DevicePort);
                    comboBox.SelectedIndex = comboBox.Items.IndexOf(get_DevicePort);
                    btn_AddNewRow.Content = "Update";
                }
            }
            catch (Exception ex)
            {
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message + " (" + DateTime.Now.ToString() + ")")));
                InsertingLogTextToLogFile(ex.Message + " (" + DateTime.Now.ToString() + ")");
                SentrySdk.CaptureException(ex);
            }

        }
        private void btnDelete_DataGrid_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsLive)
                {
                    MessageBox.Show("Changes not allowed while application \"Is Live\".", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure to Delete?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        var selectedItem = dg_Devices.SelectedItem;
                        if (selectedItem != null)
                        {
                            var getPort = ((DataRowView)selectedItem).Row.ItemArray[1].ToString();
                            DataRow DeleteRow = allPortData.Select("DevicePort='" + getPort + "'").FirstOrDefault();
                            string ColumnName = DeleteRow[0].ToString();
                            allPortData.Rows.Remove(DeleteRow);
                            allPortData.AcceptChanges();
                            dg_Devices.ItemsSource = null;
                            dg_Devices.ItemsSource = allPortData.DefaultView;
                            string text = "Table Name : " + ColumnName + " with Port " + getPort + ", has been deleted successfully.";
                            UpdateScreen(text, true, null, null);
                            BindComboBoxWithAvailablePorts();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message + " (" + DateTime.Now.ToString() + ")")));
                InsertingLogTextToLogFile(ex.Message + " (" + DateTime.Now.ToString() + ")");
                SentrySdk.CaptureException(ex);
                MessageBox.Show(ex.Message);
            }
        }        
        private void btn_CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txt_DeviceName.Text = string.Empty;
                txt_DevicePort.Text = string.Empty;
                comboBox.SelectedItem = null;
                btn_AddNewRow.Content = "Add";
                BindComboBoxWithAvailablePorts();
            }
            catch (Exception ex)
            {
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message + " (" + DateTime.Now.ToString() + ")")));
                InsertingLogTextToLogFile(ex.Message + " (" + DateTime.Now.ToString() + ")");
                SentrySdk.CaptureException(ex);
            }
        }
        #endregion

        #region GRID FUNCTIONS (BINDGRIDDATA)       
        public void BindGridData(bool isUpdate)
        {
            try
            {
                DataRow datarow;
                if (!isUpdate)
                {
                    if (allPortData == null || allPortData.Columns.Count == 0)
                    {
                        allPortData = new DataTable();
                        allPortData.Columns.Add("DeviceName", typeof(System.String));
                        allPortData.Columns.Add("DevicePort", typeof(System.String));
                    }
                    datarow = allPortData.NewRow();
                    datarow["DeviceName"] = txt_DeviceName.Text;
                    datarow["DevicePort"] = comboBox.SelectedItem.ToString();
                    allPortData.Rows.Add(datarow);
                    allPortData.AcceptChanges();
                }
                dg_Devices.ItemsSource = null;
                dg_Devices.ItemsSource = allPortData.DefaultView;
                txt_DeviceName.Text = string.Empty;
                txt_DevicePort.Text = string.Empty;
                comboBox.SelectedItem = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region BUTTONS (SAVE, QUIT, CHANGE PASSWORD) (CLICK EVENTS)        
        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsLive)
                {
                    MessageBox.Show("Changes not allowed while application \"Is Live\".", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    if (CheckFields())
                    {
                        JsonSerializerSettings jss = new JsonSerializerSettings();
                        jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                        MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure to Update?", "Update", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            var ipAddress = Crypto.Encrypt(txt_IPAddress.Text);
                            var portNumber = Crypto.Encrypt(txt_PortNumber.Text);
                            var serviceName = Crypto.Encrypt(txt_ServerName.Text);
                            var databaseName = Crypto.Encrypt(txt_DatabaseName.Text);
                            var username = Crypto.Encrypt(txt_Username.Text);
                            var password = Crypto.Encrypt(txt_Password.Password);
                            string Respone = JsonConvert.SerializeObject(allPortData, jss);
                            List<Devices> deviceData = JsonConvert.DeserializeObject<List<Devices>>(Respone);
                            if (deviceData != null && deviceData.Count > 0)
                            {
                                SaveXML(ipAddress, portNumber, serviceName, databaseName, username, password, deviceData);
                                CompareXML();
                                vtCommon.CompareData();
                                ShowChanges();
                                readXML();
                                InitializeAllPortsToSerialPorts(false);
                            }
                            else
                            {
                                string msg = "Please enter Table Name and Device Port in Device Configuration.";
                                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                txtLogs.Document.Blocks.Add(new Paragraph(new Run(msg + " (" + DateTime.Now.ToString() + ")")));
                                InsertingLogTextToLogFile(msg + " (" + DateTime.Now.ToString() + ")");
                            }
                        }
                    }
                    else
                    {
                        string msg = "Please Fill Out The Fields.";
                        MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtLogs.Document.Blocks.Add(new Paragraph(new Run(msg + " (" + DateTime.Now.ToString() + ")")));
                        InsertingLogTextToLogFile(msg + " (" + DateTime.Now.ToString() + ")");
                    }
                }
            }
            catch(Exception ex)
            {                                
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message + " (" + DateTime.Now.ToString() + ")")));
                InsertingLogTextToLogFile(ex.Message + " (" + DateTime.Now.ToString() + ")");
                SentrySdk.CaptureException(ex);
            }
        }        
        private void btn_Quit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure to logout?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message + "(" + DateTime.Now.ToString() + ")")));
                InsertingLogTextToLogFile(ex.Message + " (" + DateTime.Now.ToString() + ")");
                SentrySdk.CaptureMessage(ex.Message);
            }
        }
        private void btn_ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                win_PasswordChange win_Password = new win_PasswordChange();
                win_Password.Show();
            }
            catch (Exception ex)
            {
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message + "(" + DateTime.Now.ToString() + ")")));
                InsertingLogTextToLogFile(ex.Message + " (" + DateTime.Now.ToString() + ")");
                SentrySdk.CaptureMessage(ex.Message);
            }
        }

        #endregion

        #region FUNCTIONS (BINDCOMBOBOXWITHAVAILABLEPORTS, UPDATESCREEN, CONNECTIONTEST, CHECKFIELDS, SHOWCHANGES)
        private void BindComboBoxWithAvailablePorts()
        {
            List<string> assingedPorts = new List<string>();
            try
            {
                if (allPortData != null)
                {
                    foreach (DataRow row in allPortData.Rows)
                    {
                        assingedPorts.Add(row[1].ToString());
                    }
                }
                int num;
                string[] getPorts = SerialPort.GetPortNames().OrderBy(a => a.Length > 3 && int.TryParse(a.Substring(3), out num) ? num : 0).ToArray();
                var lst_AllPorts = getPorts.ToList();

                var availablePortsLeft = (from item in lst_AllPorts
                                          where !assingedPorts.Contains(item)
                                          select item).ToList();
                comboBox.Items.Clear();
                foreach (string comport in availablePortsLeft)
                { comboBox.Items.Add(comport); }
            }
            catch (Exception ex)
            {
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message.ToString())));
                InsertingLogTextToLogFile(ex.Message.ToString());
                SentrySdk.CaptureException(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void UpdateScreen(string Data, bool ConfigChange, string tableName, string portName)
        {
            var InsertionInTblStatus = string.Empty;
            string textMessage = string.Empty;
            string text = string.Empty;
            if (ConfigChange) {
                text = Data;
            }
            else {
                text = "ID : " + Data + " has been scanned on Port : " + portName + " with Table : " + tableName + " (" + DateTime.Now + ")";
            }
            if (IsLive == true && ConfigChange == false)
            {
                try
                {
                    TableData tblData = new TableData();
                    tblData.RFID = Convert.ToInt32(Data);
                    tblData.TableName = tableName;
                    tblData.CheckTime = DateTime.Now;

                    text = "ID : " + Data + " has been scanned on Port : " + portName + " with Table : " + tableName + " (" + DateTime.Now + ")";
                    txtLogs.Document.Blocks.Add(new Paragraph(new Run(text)));
                    InsertingLogTextToLogFile(text);

                    InsertionInTblStatus = Blayer.InsertDataToTable(tblData);
                    if (InsertionInTblStatus == "Data Inserted Successfully")
                    {
                        textMessage = "ID : " + Data + " Insert in database successfully with Port : " + portName + " & Table : " + tableName + " (" + DateTime.Now + ")";
                    }
                    else
                    {
                        textMessage = Data + " Insertion in database failed with Port : " + portName + " & Table : " + tableName + " (" + DateTime.Now + ") \nError: " + InsertionInTblStatus.Trim().ToString() + "";
                    }
                    txtLogs.Document.Blocks.Add(new Paragraph(new Run(textMessage)));
                    InsertingLogTextToLogFile(textMessage);
                }
                catch (Exception ex)
                {
                    txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message.ToString())));
                    InsertingLogTextToLogFile(ex.Message.ToString());
                    SentrySdk.CaptureException(ex);
                }
            }

            try
            {
                if (string.IsNullOrEmpty(InsertionInTblStatus))
                {
                    if (text.Length > 0)
                    {
                        txtLogs.Document.Blocks.Add(new Paragraph(new Run(text)));
                        InsertingLogTextToLogFile(text);
                    }
                }
            }
            catch (Exception ex)
            {
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message.ToString())));
                InsertingLogTextToLogFile(ex.Message.ToString());
                SentrySdk.CaptureException(ex);
            }           
        }
        public DataTable DecryptDeviceTable(DataTable dt)
        {
            if (dt != null)
            {
                var deviceTable = dt.Copy();
                int Index = 0;
                foreach (DataRow dr in deviceTable.Rows)
                {
                    foreach (DataColumn Column in deviceTable.Columns)
                    {
                        dr[Column.ColumnName] = string.IsNullOrEmpty(dr[Column.ColumnName].ToString()) ? string.Empty : Crypto.Decrypt(dr[Column.ColumnName].ToString());
                    }
                    Index++;
                }
                return deviceTable;
            }
            return null;
        }
        private bool ConnectionTest(bool fromWhere)
        {            
            var getDeviceTable = DecryptDeviceTable(vtCommon.dsXML.Tables["Devices"]);
            bool connected = true;            
            if (getDeviceTable != null)
            {
                var rows = getDeviceTable.AsEnumerable().ToList();
                if (rows.Count > 0)
                {
                    for (int i = 0; i < rows.Count; i++)
                    {
                        var getTableName = "";
                        try
                        {
                            getTableName = rows[i]["DeviceName"].ToString();
                            DataTable dt = OracleHelper.ExecuteDataset(ConnectionString, CommandType.Text, "Select * from " + getTableName + " where rownum = 1").Tables[0];
                            if (i == rows.Count - 1)
                            {
                                if (fromWhere)
                                {
                                    MessageBox.Show("Database connection successfull.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                    string text = "Database connection successfull. (" + DateTime.Now + ")";
                                    txtLogs.Document.Blocks.Add(new Paragraph(new Run(text)));
                                    InsertingLogTextToLogFile(text);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);                            
                            txtLogs.Document.Blocks.Add(new Paragraph(new Run("Error : " + ex.Message.ToString().TrimEnd('\r', '\n'))));
                            InsertingLogTextToLogFile("Error : " + ex.Message.ToString().TrimEnd('\r', '\n'));
                            SentrySdk.CaptureException(ex);
                            connected = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                string msg = "Your Port Name or Table Name not found.";
                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtLogs.Document.Blocks.Add(new Paragraph(new Run(msg + " (" + DateTime.Now.ToString() + ")")));
                InsertingLogTextToLogFile(msg + " (" + DateTime.Now.ToString() + ")");
                SentrySdk.CaptureMessage(msg);
                connected = false;
            }
            return connected;
        }
        public void clearFields()
        {
            txt_IPAddress.Text = string.Empty;
            txt_PortNumber.Text = string.Empty;
            txt_ServerName.Text = string.Empty;
            txt_DatabaseName.Text = string.Empty;
            txt_Username.Text = string.Empty;
            txt_Password.Password = string.Empty;
            dg_Devices.ItemsSource = null;
        }
        private bool CheckFields()
        {
            if (txt_IPAddress.Text.Length < 1 || txt_PortNumber.Text.Length < 1 || txt_ServerName.Text.Length < 1 || txt_Username.Text.Length < 1 || txt_Password.Password.Length < 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void ShowChanges()
        {
            if (vtCommon.dtChanges != null && vtCommon.dtChanges.Rows.Count > 0)
            {                
                foreach (DataRow dr in vtCommon.dtChanges.Rows)
                {
                    try
                    {
                        string text = string.Empty;

                        if (dr["FieldName"].ToString() == "Password")
                        {
                            text = "" + dr["FieldName"].ToString() + " : ******* has been added successfully." + DateTime.Now.ToString();
                        }
                        else if (dr["FieldName"].ToString() == "DeviceName")
                        {                            
                            var NewValueDecrypt = dr["NewValue"].ToString();
                            DataRow _NewRow = allPortData.Select("DeviceName='" + NewValueDecrypt + "'").FirstOrDefault();
                            if (_NewRow != null)
                            {
                                var lst_PortNames = _NewRow["DevicePort"].ToString();
                                text = "Table Name : " + Crypto.Decrypt(NewValueDecrypt) + " with Port " + Crypto.Decrypt(lst_PortNames) + ", has been added successfully.";
                            }
                        }
                        else if (dr["FieldName"].ToString() == "DevicePort")
                        {                            
                            var NewValueDecrypt = dr["NewValue"].ToString();
                            DataRow _NewRow = allPortData.Select("DevicePort='" + NewValueDecrypt + "'").FirstOrDefault();
                            if (_NewRow != null)
                            {                                
                                var ExistsDevicename = _NewRow["DeviceName"].ToString();
                                DataRow _ExistsRow = vtCommon.dtChanges.Select("NewValue='" + ExistsDevicename + "' AND FieldName='DeviceName'").FirstOrDefault();
                                if (_ExistsRow == null)
                                {
                                    text = "Table Name : " + Crypto.Decrypt(_NewRow["DeviceName"].ToString()) + " with Port " + Crypto.Decrypt(NewValueDecrypt) + ", has been added successfully.";
                                }
                            }
                        }
                        else
                        {
                            if (dr["FieldName"].ToString() != "DevicePort")
                            {
                                text = "" + dr["FieldName"].ToString() + " : " + Crypto.Decrypt(dr["NewValue"].ToString()) + " has been added successfully.";
                            }
                        }

                        UpdateScreen(text, true, null, null);
                    }
                    catch (Exception ex)
                    {
                        txtLogs.Document.Blocks.Add(new Paragraph(new Run(ex.Message + " (" + DateTime.Now.ToString() + ")")));
                        InsertingLogTextToLogFile(ex.Message + " (" + DateTime.Now.ToString() + ")");
                        SentrySdk.CaptureException(ex);
                    }
                }
            }
        }
        #endregion
        
        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            readXML();            
            InitializeAllPortsToSerialPorts(false);
        }
    }
}
