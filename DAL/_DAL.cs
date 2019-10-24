using RfReader_demo.BAL;
using Sentry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RfReader_demo.BAL._BAL;

namespace RfReader_demo.DAL
{
    class _DAL
    {
        public virtual string InsertDataToTable(TableData tblData)
        {
            using (SentrySdk.Init(MainWindow.conn))
            {
                try
                {
                    var dt = tblData.CheckTime.ToString("dd/MM/yyyy HH:mm:ss");
                    string query = "Insert Into " + tblData.TableName + " (RFID, ScanTime) Values ('" + tblData.RFID + "', To_Date('" + dt + "', 'DD/MM/YYYY HH24:MI:SS'))";
                    //OracleHelper.ExecuteNonQuery(_BAL.StrConnection, CommandType.Text, query).ToString();            
                    OracleHelper.ExecuteNonQuery(MainWindow.ConnectionString, CommandType.Text, query).ToString();
                    return "scan";

                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    return ex.Message;
                }
            }
        }

    }
}
