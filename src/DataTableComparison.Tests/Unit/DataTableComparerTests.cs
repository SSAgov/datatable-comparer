using PrimitiveExtensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Xunit;

namespace DataTableComparison.Tests.Unit
{
    public class DataTableComparerTests
    {

        [Fact]
        public void Add_0_expect_empty()
        {
            DataTableComparer comparer = new DataTableComparer();
            Assert.Empty(comparer.DataTables);
        }


        [Fact]
        public void Add_1_expect_1()
        {
            DataTableComparer comparer = new DataTableComparer();
            comparer.AddTable(new DataTable());
            Assert.Single(comparer.DataTables);
        }

        [Fact]
        public void Add_2_expect_2()
        {
            DataTableComparer comparer = new DataTableComparer();
            comparer.AddTable(new DataTable());
            comparer.AddTable(new DataTable());
            Assert.Equal(2, comparer.DataTables.Count);
        }

        [Fact]
        public void Add_3_expect_3()
        {
            DataTableComparer comparer = new DataTableComparer();
            comparer.AddTable(new DataTable("table1"));
            comparer.AddTable(new DataTable("table2"));
            comparer.AddTable(new DataTable("table3"));
            IEnumerable<string> tableNames = comparer.DataTables.Select(a => a.TableName);

            Assert.Equal(3, comparer.DataTables.Count);
            Assert.Contains("table1", tableNames);
            Assert.Contains("table2", tableNames);
            Assert.Contains("table3", tableNames);
        }

        [Fact]
        public void Remove_ValidRemovalOf1FromCollection_expect_2()
        {
            DataTableComparer comparer = new DataTableComparer();
            DataTable dt1 = new DataTable("table1");
            comparer.AddTable(dt1);
            comparer.AddTable(new DataTable("table2"));
            comparer.AddTable(new DataTable("table3"));
            Assert.True(comparer.RemoveTable(dt1));

            IEnumerable<string> tableNames = comparer.DataTables.Select(a => a.TableName);
            Assert.Equal(2, comparer.DataTables.Count);
            Assert.DoesNotContain("table1", tableNames);
            Assert.Contains("table2", tableNames);
            Assert.Contains("table3", tableNames);
        }

        [Fact]
        public void Remove_TableNeverAdded_expect_false()
        {
            DataTableComparer comparer = new DataTableComparer();
            DataTable dt1 = new DataTable("table1");
            Assert.False(comparer.RemoveTable(dt1));
        }

        [Fact]
        public void GetResultsDataTable_NoTables_ExpectException()
        {
            DataTableComparer comparer = new DataTableComparer();

            Assert.Throws<Exception>(() => comparer.Compare());
        }

        [Fact]
        public void GetResultsDataTable_OneTable_ExpectException()
        {
            DataTableComparer comparer = new DataTableComparer();
            comparer.AddTable(new DataTable());
            Assert.Throws<Exception>(() => comparer.Compare());
        }

        [Fact]
        public void GetResultsDataTable_TwoEmptyTables_Expect_no_rows_3_columns()
        {
            DataTableComparer comparer = new DataTableComparer();
            comparer.AddTable(new DataTable("table1"));
            comparer.AddTable(new DataTable("table2"));
            DataTableComparerResult result = comparer.Compare();
            DataTable dataTable = result.ResultsDataTable;
            IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>().Select(a => a.ColumnName);


            Assert.Contains("Exists_In_table1", columnNames);
            Assert.Contains("Exists_In_table2", columnNames);
            Assert.Contains("Exists_In_Status", columnNames);
            Assert.Empty(dataTable.Rows);
            Assert.Equal(3, dataTable.Columns.Count);
        }

        [Fact]
        public void GetResultsDataTable_TwoEmptyTables_Expect_no_rows_3_columns_customconfig()
        {
            DataTableComparerConfig config = new DataTableComparerConfig();
            config.ExistsInColumnNamePrefix = "ABC";
            DataTableComparer comparer = new DataTableComparer(config);
            comparer.AddTable(new DataTable("table1"));
            comparer.AddTable(new DataTable("table2"));
            DataTableComparerResult result = comparer.Compare();
            DataTable dataTable = result.ResultsDataTable;
            IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>().Select(a => a.ColumnName);

            Assert.Contains("ABC_table1", columnNames);
            Assert.Contains("ABC_table2", columnNames);
            Assert.Contains("ABC_Status", columnNames);
            Assert.Empty(dataTable.Rows);
            Assert.Equal(3, dataTable.Columns.Count);
        }

        [Fact]
        public void GetResultsDataTable_TwoTablesWithSameData_Expect_2_rows_in_sync()
        {
            DataTableComparerConfig config = new DataTableComparerConfig();
            config.InSyncPhrase = "true";
            config.OutOfSyncPhrase = "false";


            DataTableComparer comparer = new DataTableComparer();
            DataTable dt1 = new DataTable("table1");
            dt1.Columns.Add("Id");
            dt1.Columns.Add("Name");
            dt1.SetBestGuessPrimaryKey();
            dt1.Rows.Add("1", "Apple");
            dt1.Rows.Add("2", "Orange");

            DataTable dt2 = new DataTable("table2");
            dt2.Columns.Add("Id");
            dt2.Columns.Add("Name");
            dt2.SetBestGuessPrimaryKey();
            dt2.Rows.Add("1", "Apple");
            dt2.Rows.Add("2", "Orange");

            comparer.AddTable(dt1);
            comparer.AddTable(dt2);

            DataTableComparerResult result = comparer.Compare();
            DataTable dataTable = result.ResultsDataTable;
            IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>().Select(a => a.ColumnName);


            Assert.True(result.AllTablesContainTheSamePrimaryKeysRows());
            Assert.Contains("Id", columnNames);
            Assert.Contains("Exists_In_table1", columnNames);
            Assert.Contains("Exists_In_table2", columnNames);
            Assert.Contains("Exists_In_Status", columnNames);
            Assert.Contains("Name_Status", columnNames);
            Assert.Contains("Name_table1", columnNames);
            Assert.Contains("Name_table2", columnNames);
            Assert.Equal(2, dataTable.Rows.Count);
        }
        [Fact]
        public void GetResultsDataTable_TwoTablesOneHasExtraRow_Expect_3()
        {
            DataTableComparerConfig config = new DataTableComparerConfig();
            config.InSyncPhrase = "true";
            config.OutOfSyncPhrase = "false";


            DataTableComparer comparer = new DataTableComparer();
            DataTable dt1 = new DataTable("table1");
            dt1.Columns.Add("Id");
            dt1.Columns.Add("Name");
            dt1.SetBestGuessPrimaryKey();
            dt1.Rows.Add("1", "Apple");
            dt1.Rows.Add("2", "Orange");

            DataTable dt2 = new DataTable("table2");
            dt2.Columns.Add("Id");
            dt2.Columns.Add("Name");
            dt2.SetBestGuessPrimaryKey();
            dt2.Rows.Add("1", "Apple");
            dt2.Rows.Add("2", "Orange");
            dt2.Rows.Add("3", "Orange");

            comparer.AddTable(dt1);
            comparer.AddTable(dt2);

            DataTableComparerResult result = comparer.Compare();
            DataTable dataTable = result.ResultsDataTable;
            IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>().Select(a => a.ColumnName);

            Assert.False(result.AllTablesContainTheSamePrimaryKeysRows());
            Assert.Contains("Id", columnNames);
            Assert.Contains("Exists_In_table1", columnNames);
            Assert.Contains("Exists_In_table2", columnNames);
            Assert.Contains("Exists_In_Status", columnNames);
            Assert.Contains("Name_Status", columnNames);
            Assert.Contains("Name_table1", columnNames);
            Assert.Contains("Name_table2", columnNames);
            Assert.Equal(3, dataTable.Rows.Count);
        }
    }
}
