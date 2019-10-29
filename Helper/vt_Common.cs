using Sentry;
using System;
using System.Collections.Generic;
using System.Data;

namespace RfReader_demo.Helper
{
    class vt_Common
    {
        public DataSet dsXML = new DataSet();
        public DataSet dsNew = new DataSet();
        public DataSet dsForDevices = new DataSet();
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
                    int Index = 0;
                    foreach (DataRow dr in dtCurrent.Rows)
                    {
                        foreach (DataColumn Column in dtCurrent.Columns)
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
                            if (dtCurrent.TableName == "test-Devices")
                            {
                                var dtCurrent_ColumnValue = dr[Column.ColumnName].ToString();
                                var dtLast_ColumnValue = string.Empty;
                                try
                                {
                                    dtLast_ColumnValue = dtLast.Rows[Index][Column.ColumnName].ToString();
                                    if (dtCurrent_ColumnValue != dtLast_ColumnValue)
                                    {
                                        AddNewCreatedRow(Column.ColumnName, dtLast.Rows[Index][Column.ColumnName].ToString(), dr[Column.ColumnName].ToString());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    AddNewCreatedRow(Column.ColumnName, string.Empty, dr[Column.ColumnName].ToString());
                                }
                            }
                            else
                            {
                                if (dr[Column.ColumnName].ToString() != dtLast.Rows[Index][Column.ColumnName].ToString())
                                {
                                    AddNewCreatedRow(Column.ColumnName, dtLast.Rows[Index][Column.ColumnName].ToString(), dr[Column.ColumnName].ToString());
                                }
                            }
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
                        DataTable SecondDataTable = new DataTable();
                        DataTable FirstDataTable = table;
                        var getNewTableName = table.TableName.ToString();
                        if (getNewTableName == "Devices")
                        {
                            SecondDataTable = dsForDevices.Tables[table.TableName];
                        }
                        else
                        {
                            SecondDataTable = dsXML.Tables[table.TableName];
                        }
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
