using System.Diagnostics;

namespace EFCore.Toolkit.Extensions
{
    [DebuggerDisplay("TableRowCounts: {this.TableName}, Count={this.TableRowCount}")]
    public class TableRowCounts
    {
        public string TableName { get; set; }

        public int TableRowCount { get; set; }
    }
}