using RfReader_demo.BAL;
using Sentry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using static RfReader_demo.BAL._BAL;

namespace RfReader_demo.DAL
{
    class _DAL
    {
        public virtual string InsertDataToTable(TableData tblData)
        {
            string Message = string.Empty;
            try
            {
                string conn = ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
                if (!string.IsNullOrEmpty(conn))
                {
                    SentrySdk.Init(conn);
                }
                var dt = tblData.CheckTime.ToString("dd/MM/yyyy HH:mm:ss");
                string query = "Insert Into " + tblData.TableName + " (RFID, ScanTime) Values ('" + tblData.RFID + "', To_Date('" + dt + "', 'DD/MM/YYYY HH24:MI:SS'))";        
                int ID =  OracleHelper.ExecuteNonQuery(MainWindow.ConnectionString, CommandType.Text, query);
                if (ID == 1)
                {
                    Message = "Data Inserted Successfully";
                }
                else
                {
                    Message = "Cannot Insert data in table";
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                SentrySdk.CaptureException(ex);
            }
            return Message;
        }

    }
}
