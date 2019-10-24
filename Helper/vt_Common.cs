using Sentry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                                var columnName = dr[Column.ColumnName].ToString();
                                var lastColumnName = dtLast.Rows[Index][Column.ColumnName].ToString();
                                var columnName2 = Column.ColumnName;

                                DataRow ddr = dtChanges.NewRow();
                                ddr["FieldName"] = Column.ColumnName;
                                ddr["OldValue"] = dtLast.Rows[Index][Column.ColumnName].ToString();
                                ddr["NewValue"] = dr[Column.ColumnName].ToString();
                                dtChanges.Rows.Add(ddr);
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
                                    string colname2 = Column.ColumnName[i].ToString();
                                    string colname = dtCurrent.Columns[i].ColumnName.ToString();
                                    dtLast.Columns.Add(colname, typeof(System.String));

                                    DataRow datarow = dtLast.NewRow();
                                    dtLast.Rows.Add(datarow);
                                }

                                if (dtCurrent.Rows.Count != dtLast.Rows.Count)
                                {
                                    if (dtCurrent.Rows.Count > dtLast.Rows.Count)
                                    {
                                        for (int i = 0; ; i++)
                                        {
                                            DataRow datarow = dtLast.NewRow();
                                            dtLast.Rows.Add(datarow);
                                            if (dtCurrent.Rows.Count == dtLast.Rows.Count) { break; }
                                        }
                                    }
                                    if (dtCurrent.Rows.Count < dtLast.Rows.Count)
                                    {
                                        for (int i = dtLast.Rows.Count; ; i--)
                                        {
                                            DataRowCollection datarow = dtLast.Rows;
                                            datarow[i - 1].Delete();
                                            if (dtCurrent.Rows.Count == dtLast.Rows.Count) { break; }
                                        }
                                    }
                                }
                            }

                            if (dtCurrent.TableName == "test-Devices")
                            {
                                
                                    var dtCurrent_ColumnValue = dr[Column.ColumnName].ToString();
                                    var dtLast_ColumnValue = "";
                                    try
                                    {
                                        dtLast_ColumnValue = dtLast.Rows[Index][Column.ColumnName].ToString();
                                        if (dtCurrent_ColumnValue != dtLast_ColumnValue)
                                        {
                                            DataRow ddr = dtChanges.NewRow();
                                            ddr["FieldName"] = Column.ColumnName;
                                            ddr["OldValue"] = dtLast.Rows[Index][Column.ColumnName].ToString();
                                            ddr["NewValue"] = dr[Column.ColumnName].ToString();
                                            dtChanges.Rows.Add(ddr);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        if (ex.Message.Contains("There is no row at position"))
                                        {
                                            DataRow ddr = dtChanges.NewRow();
                                            ddr["FieldName"] = Column.ColumnName;
                                            ddr["OldValue"] = "";
                                            ddr["NewValue"] = dr[Column.ColumnName].ToString();
                                            dtChanges.Rows.Add(ddr);
                                        }
                                    }
                            }
                            else
                            {
                                if (dr[Column.ColumnName].ToString() != dtLast.Rows[Index][Column.ColumnName].ToString())
                                {
                                    var columnName = dr[Column.ColumnName].ToString();
                                    var lastColumnName = dtLast.Rows[Index][Column.ColumnName].ToString();
                                    var columnName2 = Column.ColumnName;

                                    DataRow ddr = dtChanges.NewRow();
                                    ddr["FieldName"] = Column.ColumnName;
                                    ddr["OldValue"] = dtLast.Rows[Index][Column.ColumnName].ToString();
                                    ddr["NewValue"] = dr[Column.ColumnName].ToString();
                                    dtChanges.Rows.Add(ddr);
                                }
                            }
                        }
                        Index++;
                    }
                }
                if (dtLast.Rows.Count > dtCurrent.Rows.Count)
                {
                    int Index = 0;
                    foreach (DataRow dr in dtLast.Rows)
                    {
                        foreach (DataColumn Column in dtLast.Columns)
                        {
                            if (dtLast == null)
                            {
                                dtLast = new DataTable();

                                for (int i = 0; i < dtCurrent.Columns.Count; i++)
                                {
                                    string colname2 = Column.ColumnName[i].ToString();
                                    string colname = dtCurrent.Columns[i].ColumnName.ToString();
                                    dtLast.Columns.Add(colname, typeof(System.String));

                                    DataRow datarow = dtLast.NewRow();
                                    dtLast.Rows.Add(datarow);
                                }

                                if (dtCurrent.Rows.Count != dtLast.Rows.Count)
                                {
                                    if (dtCurrent.Rows.Count > dtLast.Rows.Count)
                                    {
                                        for (int i = 0; ; i++)
                                        {
                                            DataRow datarow = dtLast.NewRow();
                                            dtLast.Rows.Add(datarow);
                                            if (dtCurrent.Rows.Count == dtLast.Rows.Count) { break; }
                                        }
                                    }
                                }
                            }

                            if (dtCurrent.TableName == "test-Devices")
                            {
                                    var dtLast_ColumnValue = dr[Column.ColumnName].ToString();
                                    var dtCurrent_ColumnValue = "";
                                    
                                    try
                                    {
                                        
                                        dtCurrent_ColumnValue = dtCurrent.Rows[Index][Column.ColumnName].ToString(); //dr[Column.ColumnName].ToString();
                                        if (dtLast_ColumnValue != dtCurrent_ColumnValue)
                                        {
                                            DataRow ddr = dtChanges.NewRow();
                                            ddr["FieldName"] = Column.ColumnName;
                                            ddr["OldValue"] = dtLast.Rows[Index][Column.ColumnName].ToString();
                                            ddr["NewValue"] = dr[Column.ColumnName].ToString();
                                            dtChanges.Rows.Add(ddr);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        if (ex.Message.Contains("There is no row at position"))
                                        {
                                            DataRow ddr = dtChanges.NewRow();
                                            ddr["FieldName"] = Column.ColumnName;
                                            ddr["OldValue"] = "";
                                            ddr["NewValue"] = dr[Column.ColumnName].ToString();
                                            dtChanges.Rows.Add(ddr);
                                        }
                                    }
                            }
                            else
                            {
                                if (dr[Column.ColumnName].ToString() != dtLast.Rows[Index][Column.ColumnName].ToString())
                                {
                                    var columnName = dr[Column.ColumnName].ToString();
                                    var lastColumnName = dtLast.Rows[Index][Column.ColumnName].ToString();
                                    var columnName2 = Column.ColumnName;

                                    DataRow ddr = dtChanges.NewRow();
                                    ddr["FieldName"] = Column.ColumnName;
                                    ddr["OldValue"] = dtLast.Rows[Index][Column.ColumnName].ToString();
                                    ddr["NewValue"] = dr[Column.ColumnName].ToString();
                                    dtChanges.Rows.Add(ddr);
                                }
                            }
                        }
                        Index++;
                    }
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureMessage(ex.Message);
                throw ex;                
            }
        }
        public DataTable CompareData()
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
                for (int i = 0; i < 3; i++)
                {
                    DataTable table2 = dsNew.Tables[0];
                    if (dsNew.Tables.CanRemove(table2))
                    {
                        dsNew.Tables.Remove(table2);
                    }
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureMessage(ex.Message);
            }
            return dtChanges;
        }
    }

    public class Devices
    {
        public string DeviceName { get; set; }
        public string DevicePort { get; set; }

    }
}
