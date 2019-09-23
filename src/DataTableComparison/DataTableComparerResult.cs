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
    public class DataTableComparerResult
    {
        public DataTableComparer DataTableComparer { get; internal set; }
        public DataTable ResultsDataTable { get; internal set; }
        public bool AllTablesContainTheSamePrimaryKeysRows()
        {
            return ResultsDataTable.Select($"[{DataTableComparer.Config.ExistsInColumnNamePrefix}{DataTableComparer.Config.WordSeperator}Status] = '{DataTableComparer.Config.OutOfSyncPhrase}'")
                .Count() == 0;
        }

        public DataTableComparerResult(DataTableComparer dataTableComparer)
        {
            DataTableComparer = dataTableComparer;
        }
    }
}
