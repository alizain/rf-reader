using Sentry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RfReader_demo.Helper
{
    class vt_Common
    {
        public DataSet dsXML = new DataSet();
        public DataSet dsNew = new DataSet();
        public DataTable dtChanges = new DataTable();

        public void getDifferentRecords(DataTable dtCurrent, DataTable dtLast)
        {
            try
            {
                int dtCurrentCount = dtCurrent == null ? 0 : dtCurrent.Rows.Count;
                int dtLastCount = dtLast == null ? 0 : dtLast.Rows.Count;

                if (dtCurrentCount == dtLastCount)
                {
                    int Index = 0;
                    foreach (DataRow dr in dtCurrent.Rows)
                    {
                        foreach (DataColumn Column in dtCurrent.Columns)
                        {
                            if (dr[Column.ColumnName].ToString() != dtLast.Rows[Index][Column.ColumnName].ToString())
                            {
                                AddNewCreatedRow(Column.ColumnName, dtLast.Rows[Index][Column.ColumnName].ToString(), dr[Column.ColumnName].ToString());
                            }
                        }
                        Index++;
                    }
                }
                if (dtCurrentCount > dtLastCount)
                {
                    if (dtLast == null)
                    {
                        dtLast = new DataTable();
                        for (int i = 0; i < dtCurrent.Columns.Count; i++)
                        {
                            string colname = dtCurrent.Columns[i].ColumnName.ToString();
                            dtLast.Columns.Add(colname, typeof(System.String));
                        }
                        if (dtCurrent.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtCurrent.Rows.Count; i++)
                            {
                                DataRow datarow = dtLast.NewRow();
                                dtLast.Rows.Add(datarow);
                            }
                        }
                        dtLast.AcceptChanges();
                    }

                    if (dtCurrent.TableName != "test-Devices")
                    {
                        int count = 0;
                        foreach (DataRow dr in dtCurrent.Rows)
                        {
                            foreach (DataColumn Column in dtCurrent.Columns)
                            {
                                AddNewCreatedRow(Column.ColumnName, dtCurrent.Rows[count][Column.ColumnName].ToString(), dr[Column.ColumnName].ToString());                            
                            }
                            count++;
                        }
                    }
                    else
                    {
                        DataTable TableC = new DataTable();
                        var idsNotInB = dtCurrent.AsEnumerable().Select(r => r.Field<string>("DevicePort"))
                            .Except(dtLast.AsEnumerable().Select(r => r.Field<string>("DevicePort")));
                        TableC = (from row in dtCurrent.AsEnumerable()
                                  join id in idsNotInB
                                  on row.Field<string>("DevicePort") equals id
                                  select row).CopyToDataTable();

                        int Index = 0;
                        foreach (DataRow dr in TableC.Rows)
                        {
                            foreach (DataColumn Column in TableC.Columns)
                            {
                                var columnName = Column.ColumnName.ToString();
                                var columnValue = TableC.Rows[Index][Column.ColumnName].ToString();
                                AddNewCreatedRow(columnName, columnValue, columnValue);
                            }
                            Index++;
                        }
                    }
                }
                if (dtLastCount > dtCurrentCount)
                {                                        
                    DataTable TableC_ = new DataTable();

                    var idsNotInB_ = dtLast.AsEnumerable().Select(r => r.Field<string>("DevicePort"))
                        .Except(dtCurrent.AsEnumerable().Select(r => r.Field<string>("DevicePort")));
                    TableC_ = (from row in dtLast.AsEnumerable()
                              join id in idsNotInB_
                              on row.Field<string>("DevicePort") equals id
                              select row).CopyToDataTable();
                    int Index = 0;
                    foreach (DataRow dr in TableC_.Rows)
                    {
                        foreach (DataColumn Column in TableC_.Columns)
                        {
                            var columnName = Column.ColumnName.ToString();                             
                            var columnValue = TableC_.Rows[Index][Column.ColumnName].ToString();
                            AddNewCreatedRow(columnName, columnValue, columnValue);                           
                        }
                        Index++;
                    }                                        
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void AddNewCreatedRow(string FieldName, string OldValue, string NewValue)
        {
            try
            {
                DataRow ddr = dtChanges.NewRow();
                ddr["FieldName"] = FieldName;
                ddr["OldValue"] = OldValue.ToString();
                ddr["NewValue"] = NewValue.ToString();
                dtChanges.Rows.Add(ddr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void CompareData()
        {
            try
            {
                if (dsNew.Tables.Count > 0)
                {
                    dtChanges = new DataTable();
                    dtChanges.Columns.Add("FieldName");
                    dtChanges.Columns.Add("OldValue");
                    dtChanges.Columns.Add("NewValue");
                    foreach (DataTable table in dsNew.Tables)
                    {
                        DataTable SecondDataTable = dsXML.Tables[table.TableName];
                        DataTable FirstDataTable = table;                        
                        FirstDataTable.TableName = "test-" + table;
                        getDifferentRecords(FirstDataTable, SecondDataTable);
                    }
                }
                dsNew = new DataSet();
            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }
    }
    public class Devices
    {
        public string DeviceName { get; set; }
        public string DevicePort { get; set; }
    }
}
