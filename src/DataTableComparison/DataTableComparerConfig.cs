using PrimitiveExtensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataTableComparison
{
    public class DataTableComparerConfig
    {
        
        public DataTableComparerConfig(string inSyncPhrase = "In Sync", string outOfSyncPhrase = "Out of Sync", string existsInColumnName = "Exists_In", string wordSeperator = "_")
        {
            InSyncPhrase = inSyncPhrase;
            OutOfSyncPhrase = outOfSyncPhrase;
            ExistsInColumnNamePrefix = existsInColumnName;
            WordSeperator = wordSeperator;
        }

        public bool ColumnNamesCaseSensitive { get; set; }
        public bool DataFieldValuesCaseSensitive { get; set; }
        public bool IncludeUniqueColumnsInOutput { get; set; }

        public string InSyncPhrase { get; set; }
        public string OutOfSyncPhrase { get; set; }
        public string ExistsInColumnNamePrefix { get; set; }
        public string WordSeperator { get; set; }
    }
}
