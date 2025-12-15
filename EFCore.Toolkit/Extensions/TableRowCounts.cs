using System.Diagnostics;

namespace EFCore.Toolkit.Extensions
{
    [DebuggerDisplay("{this.TableName}, Count={this.TableRowCount}", Type = "TableRowCounts")]
    public class TableRowCounts
    {
        public string TableName { get; set; } = null!;

        public int TableRowCount { get; set; }
    }
}