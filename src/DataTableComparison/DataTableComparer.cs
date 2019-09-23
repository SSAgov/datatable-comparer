using SSAx.PrimitiveExtensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataTableComparison
{
    /// <summary>
    /// Helper class to compare multiple data tables
    /// </summary>
    public class DataTableComparer
    {
        public DataTableComparer()
        {
            DataTables = new List<DataTable>();
            Config = new DataTableComparerConfig();
        }

        public DataTableComparer(DataTableComparerConfig config)
            : this()
        {
            Config = config;
        }

        public DataTableComparerConfig Config { get; set; }

        public List<DataTable> DataTables { get; set; }

        public DataTableComparerResult Compare()
        {
            DataTableComparerResult result = new DataTableComparerResult(this);
            result.ResultsDataTable = GetResultsDataTable();
            return result;
        }

        internal DataTable GetResultsDataTable()
        {
            if (DataTables.Count < 2)
                throw new Exception("Must have at least 2 tables added to compare");


            List<DataTable> dtsTemp = new List<DataTable>();
            List<DataTable> dts = new List<DataTable>();
            DataTable dtResult = new DataTable();
            dtResult.CaseSensitive = Config.DataFieldValuesCaseSensitive;

            DataTables.CopyTo(dts);

            foreach (DataTable dt in dts)
                dtsTemp.Add(dt);


            if (dts != null && dts.Count() > 0)
            {
                DataTable dtTemp = new DataTable();
                List<DataTable> dtsCopy = new List<DataTable>();
                dts.CopyTo(dtsCopy);

                foreach (DataTable dt in dtsCopy)
                {
                    dt.AddColumnIfNotExists(Config.ExistsInColumnNamePrefix, typeof(string));
                    foreach (DataRow r in dt.Rows)
                        r[Config.ExistsInColumnNamePrefix] = "Yes";
                }

                DataColumn[] keys = new DataColumn[dtsCopy[0].PrimaryKey.Length];
                for (int i = 0; i < dtsCopy[0].PrimaryKey.Length; i++)
                {
                    try
                    {
                        Type t = dtsCopy[0].PrimaryKey[i].DataType;
                        dtResult.AddColumnIfNotExists(dtsCopy[0].PrimaryKey[i].ColumnName, t);//typeof(string));
                        keys[i] = dtResult.Columns[dtsCopy[0].PrimaryKey[i].ColumnName];
                        ////     result.CommonKeyColumnNames.Add(dtsCopy[0].PrimaryKey[i].ColumnName);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                dtResult.PrimaryKey = keys;
                List<string> tableNames = new List<string>();
                List<string> commonNonKeyColumns = dtsCopy.GetColumnNames_Shared_NonPrimaryKey().ToList();
                List<string> uniqueNonKeyColumns = dtsCopy.GetColumnNames_NotShared_NonPrimaryKey().ToList();

                foreach (DataTable dt in dtsCopy)
                {
                    tableNames.Add(dt.TableName);
                    foreach (DataColumn col in dt.Columns)
                    {
                        col.AllowDBNull = true;
                        if (commonNonKeyColumns.IndexOf(col.ColumnName) >= 0)
                        {
                            col.ColumnName = col.ColumnName + Config.WordSeperator + dt.TableName;
                        }
                    }
                    foreach (DataColumn col in dtResult.Columns)
                    {
                        col.AllowDBNull = true;
                    }
                    try
                    {
                        dtResult.Merge(dt);
                    }
                    catch (Exception ex)
                    {
                        dtResult.Merge(dt, true, MissingSchemaAction.AddWithKey);
                        throw ex;
                    }
                }

                string expression = GetKeyStatusExpression(dtsCopy);
                dtResult.AddColumnIfNotExists($"{Config.ExistsInColumnNamePrefix}{Config.WordSeperator}Status", typeof(string), expression);

                foreach (string commonNonKeyColumn in commonNonKeyColumns)
                {
                    expression = GetNonKeyStatusExpression(dtsCopy, commonNonKeyColumn, dtResult);
                    if (expression != "")
                    {
                        dtResult.AddColumnIfNotExists($"{commonNonKeyColumn}{Config.WordSeperator}Status", typeof(string), expression, false);
                    }
                }

                commonNonKeyColumns.Remove(Config.ExistsInColumnNamePrefix);
                foreach (string commonNonKeyColumn in commonNonKeyColumns)
                {
                    foreach (string tableName in tableNames)
                    {
                        dtResult.SetColumnOrdinalIfExists(commonNonKeyColumn + Config.WordSeperator + tableName, dtResult.Columns.Count - 1);
                    }
                }

                foreach (string uniqueNonKeyColumn in uniqueNonKeyColumns)
                {
                    foreach (DataTable dt in dtsCopy)
                    {
                        if (Config.IncludeUniqueColumnsInOutput)
                        {
                            if (dt.Columns.Contains(uniqueNonKeyColumn))
                            {
                                dtResult.SetColumnOrdinalIfExists(uniqueNonKeyColumn, dtResult.Columns.Count - 1);
                                dtResult.RenameColumnIfExists(uniqueNonKeyColumn, uniqueNonKeyColumn + " " + dt.TableName);
                            }
                        }
                        else
                        {
                            dtResult.DeleteColumnIfExists(uniqueNonKeyColumn);
                        }
                    }
                }

                dtResult.TableName = (dtResult.Namespace + " Comparison Results").Trim();
                dtResult.AcceptChanges();
                dtsTemp.Clear();
            }
            return dtResult;
        }


        /// <summary>
        /// Return the comparison restults in a table
        /// </summary>
        /// <param name="includeUniqueColumns"></param>
        /// <returns></returns>
        //public DaataTableComparerResult Compare()
        //{
        //    DaataTableComparerResult result = new DaataTableComparerResult(this);
        //    List<DataTable> dtsTemp = new List<DataTable>();
        //    List<DataTable> dts = new List<DataTable>();

        //    result.CommonColumnNames.AddRange(_DataTableList.GetColumnNames_Shared_All());
        //    result.AllColumnNamesIdentical = _DataTableList.GetColumnNames_NotShared_All().Count() == 0;

        //    if (_DataTableList.Count < 2)
        //        throw new Exception("Must have at least 2 tables added to compare");

        //    _DataTableList.CopyTo(dts);
        //    //Add a CompareResult for each Collection ModelID

        //    foreach (DataTable dt in dts)
        //        dtsTemp.Add(dt);

        //    // DataTableCompareResult _result = DataTableHelper.CompareDataTables(dtsTemp, CaseSensitive,includeUniqueColumns);
        //    // public static DataTableCompareResult CompareDataTables(List<DataTable> dts, bool caseSensitive = false, bool includeUniqueColumns = false)

        //    if (dts != null && dts.Count() > 0)
        //    {
        //        //////List<int> indexesOracleDataTables = new List<int>();
        //        DataTable dtTemp = new DataTable();
        //        List<DataTable> dtsCopy = new List<DataTable>();

        //        dts.CopyTo(dtsCopy);

        //        foreach (DataTable dt in dtsCopy)
        //        {
        //            dt.AddColumnIfNotExists("Exists In", typeof(string));
        //            foreach (DataRow r in dt.Rows)
        //                r["Exists In"] = "Yes";

        //            //////if (dt.TableName.Length >= 3)
        //            //////{
        //            //////    if (dt.TableName.ToString().Substring(0, 3) == "ORA")
        //            //////    {
        //            //////        indexesOracleDataTables.Add(dtsCopy.IndexOf(dt));
        //            //////    }
        //            //////}
        //        }

        //        //Create a new DataTable that will be a merge of all the
        //        DataTable dtResult = new DataTable();
        //        dtResult.CaseSensitive = _CaseSensitive;
        //        DataColumn[] keys = new DataColumn[dtsCopy[0].PrimaryKey.Length];
        //        for (int i = 0; i < dtsCopy[0].PrimaryKey.Length; i++)
        //        {
        //            try
        //            {
        //                Type t = dtsCopy[0].PrimaryKey[i].DataType;
        //                dtResult.AddColumnIfNotExists(dtsCopy[0].PrimaryKey[i].ColumnName, t);//typeof(string));
        //                keys[i] = dtResult.Columns[dtsCopy[0].PrimaryKey[i].ColumnName];
        //                result.CommonKeyColumnNames.Add(dtsCopy[0].PrimaryKey[i].ColumnName);
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }

        //        dtResult.PrimaryKey = keys;
        //        List<string> tableNames = new List<string>();
        //        List<string> commonNonKeyColumns = dtsCopy.GetColumnNames_Shared_NonPrimaryKey().ToList();
        //        List<string> uniqueNonKeyColumns = dtsCopy.GetColumnNames_NotShared_NonPrimaryKey().ToList();

        //        foreach (DataTable dt in dtsCopy)
        //        {
        //            tableNames.Add(dt.TableName);
        //            foreach (DataColumn col in dt.Columns)
        //            {
        //                col.AllowDBNull = true;
        //                if (commonNonKeyColumns.IndexOf(col.ColumnName) >= 0)
        //                {
        //                    col.ColumnName = col.ColumnName + " " + dt.TableName;
        //                }
        //            }
        //            foreach (DataColumn col in dtResult.Columns)
        //            {
        //                col.AllowDBNull = true;
        //            }
        //            try
        //            {
        //                dtResult.Merge(dt);
        //            }
        //            catch (Exception ex)
        //            {
        //                dtResult.Merge(dt, true, MissingSchemaAction.AddWithKey);
        //                throw ex;
        //            }
        //        }

        //        string expression = GetKeyStatusExpression(dtsCopy);
        //        dtResult.AddColumnIfNotExists("Exists In Status", typeof(string), expression);

        //        foreach (string commonNonKeyColumn in commonNonKeyColumns)
        //        {
        //            expression = GetNonKeyStatusExpression(dtsCopy, commonNonKeyColumn + " ", dtResult);
        //            if (expression != "")
        //            {
        //                dtResult.AddColumnIfNotExists(commonNonKeyColumn + " Status", typeof(string), expression, false);
        //            }
        //        }

        //        commonNonKeyColumns.Remove("Exists In");
        //        foreach (string commonNonKeyColumn in commonNonKeyColumns)
        //        {
        //            foreach (string tableName in tableNames)
        //            {
        //                dtResult.SetColumnOrdinalIfExists(commonNonKeyColumn + " " + tableName, dtResult.Columns.Count - 1);
        //            }
        //        }

        //        foreach (string uniqueNonKeyColumn in uniqueNonKeyColumns)
        //        {
        //            foreach (DataTable dt in dtsCopy)
        //            {
        //                if (Config.IncludeUniqueColumnsInOutput)
        //                {
        //                    if (dt.Columns.Contains(uniqueNonKeyColumn))
        //                    {
        //                        dtResult.SetColumnOrdinalIfExists(uniqueNonKeyColumn, dtResult.Columns.Count - 1);
        //                        dtResult.RenameColumnIfExists(uniqueNonKeyColumn, uniqueNonKeyColumn + " " + dt.TableName);
        //                    }
        //                }
        //                else
        //                {
        //                    dtResult.DeleteColumnIfExists(uniqueNonKeyColumn);
        //                }
        //            }
        //        }

        //        dtResult.TableName = (dtResult.Namespace + " Comparison Results").Trim();
        //        dtResult.AcceptChanges();

        //        result.UniqueNonKeyColumnNames = uniqueNonKeyColumns;
        //        result.CommonNonKeyColumnNames = commonNonKeyColumns;
        //        result.ResultDataTable = dtResult;
        //    }


        //    dtsTemp.Clear();
        //    return result;
        //}

        public void AddTable(DataTable dt)
        {
            if (dt.TableName == "")
                dt.TableName = $"Table_{new Random().Next(1, 1000)}";

            int i = 1;
            string tbName = dt.TableName;
            while (DataTables.ContainsTableName(tbName))
            {
                tbName = dt.TableName + "_" + i;
                i++;
            }
            dt.TableName = tbName;

            DataTables.Add(dt);
            //foreach (DataColumn c in dt.Columns)
            //{
            //    c.ColumnName = c.ColumnName.ToUpper();
            //}

            //if (DataTableHelper.DataTableExists(_DataTableList,dt.Name))
            //{

            //throw new Exception("This item or one just like it already exists in the compare.");
            //}

            //TODO Build the Collection ModelID List based the table'_Locator namespace


            //NotifyTableAdded();
        }
        public bool RemoveTable(DataTable datatable)
        {
            return DataTables.Remove(datatable);
        }

        //private void NotifyTableAdded()
        //{
        //    if (DataTableAdded != null)
        //    {
        //        DataTableAdded(this, EventArgs.Empty);
        //    }
        //}

        //public void RemoveAt(int i)
        //{
        //    try
        //    {
        //        DataTables.RemoveAt(i);
        //        if (DataTableRemoved != null)
        //        {
        //            DataTableRemoved(this, EventArgs.Empty);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public void Clear()
        //{

        //    DataTables.Clear();
        //    // TODO EVENTBD cleared
        //    if (DataTableCollectionCleared != null)
        //    {
        //        DataTableCollectionCleared(this, EventArgs.Empty);
        //    }
        //}


        private string GetKeyStatusExpression(List<DataTable> dts)
        {
            string sExpression = "";
            List<string> tableNames = new List<string>();
            foreach (DataTable dt in dts)
            {
                tableNames.Add(dt.TableName);
            }

            foreach (string tableName in tableNames)
            {
                if (tableNames.IndexOf(tableName) == 0)
                {
                    sExpression = $"isnull([{Config.ExistsInColumnNamePrefix}{Config.WordSeperator}{tableName}],'0')='0'";
                }
                else
                {
                    sExpression = sExpression + $" OR isnull([{Config.ExistsInColumnNamePrefix}{Config.WordSeperator}{tableName}],'0')='0'";
                }
            }
            sExpression = $"iif({sExpression},'{Config.OutOfSyncPhrase}','{Config.InSyncPhrase}')";

            return sExpression;
        }

        private string GetNonKeyStatusExpression(List<DataTable> dts, string columnName, DataTable dtResults)
        {
            string sExpression = "";
            List<string> tableNames = new List<string>();
            foreach (DataTable dt in dts)
            {
                tableNames.Add(columnName + Config.WordSeperator + dt.TableName);
            }

            foreach (string tableName in tableNames)
            {
                Type t = dtResults.Columns[tableName].DataType;

                switch (Type.GetTypeCode(t))
                {
                    case TypeCode.DBNull:

                        break;

                    case TypeCode.DateTime:
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.SByte:
                    case TypeCode.Single:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        if (tableNames.IndexOf(tableName) == 0)
                        {
                            sExpression = $"isnull([{tableName}],0) = isnull([{tableNames[tableNames.IndexOf(tableName) + 1]}],0)";
                            //throwing error on 1/4/2017{"Index was out of range. Must be non-negative and less than the size of the collection.\r\nParameter name: index"}
                        }
                        else
                        {
                            if (tableNames.IndexOf(tableName) == tableNames.Count - 1)
                            {

                            }
                            else
                            {
                                sExpression = sExpression + $" AND Isnull([{tableName}],0) = isnull([{tableNames[tableNames.IndexOf(tableName) + 1]}],0)";
                            }
                        }
                        break;

                    case TypeCode.Empty:
                    case TypeCode.Char:
                    case TypeCode.String:
                        if (tableNames.IndexOf(tableName) == 0)
                        {
                            sExpression = $"isnull([{tableName}],'') = isnull([{tableNames[tableNames.IndexOf(tableName) + 1]}],'')";
                            //throwing error on 1/4/2017{"Index was out of range. Must be non-negative and less than the size of the collection.\r\nParameter name: index"}
                        }
                        else
                        {
                            if (tableNames.IndexOf(tableName) == tableNames.Count - 1)
                            {

                            }
                            else
                            {
                                sExpression = sExpression + $" AND isnull([{tableName}],'') = isnull([{tableNames[tableNames.IndexOf(tableName) + 1]}],'')";
                            }
                        }
                        break;
                    default:
                        return "";
                }
            }
            sExpression = $"iif([{Config.ExistsInColumnNamePrefix}{Config.WordSeperator}Status]='{Config.InSyncPhrase}',iif({sExpression},'{Config.InSyncPhrase}','{Config.OutOfSyncPhrase}'),'')";
            return sExpression;
        }
    }

    //public List<DataTable> GetDataTables(string CollectionName = null)
    //{

    //    if (CollectionName == null)
    //    {
    //        return _DataTableList;
    //    }
    //    else
    //    {
    //        List<DataTable> dataTableListFiltered = new List<DataTable>();
    //        dataTableListFiltered.Clear();

    //        foreach (DataTable dt in _DataTableList)
    //        {
    //            dataTableListFiltered.Add(dt.Copy());
    //        }

    //        return dataTableListFiltered;
    //    }
    //}

}
