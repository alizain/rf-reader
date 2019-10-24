using RfReader_demo.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OracleClient;

namespace RfReader_demo.BAL
{
    class _BAL : _DAL
    {
        public class TableData
        {
            public int RFID { get; set; }
            public DateTime CheckTime { get; set; }
            public string TableName { get; set; }
        }

        public override string InsertDataToTable(TableData tblData)
        {
            return base.InsertDataToTable(tblData);
        }
    }
}
