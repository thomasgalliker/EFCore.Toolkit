using System.Diagnostics;

namespace EFCore.Toolkit.Abstractions
{
    [DebuggerDisplay("ChangeSet: Context='{this.Context.Name}', Changes={this.Changes.Count()}", Type = "Change")]
    public class ChangeSet
    {
        public ChangeSet(Type contextType, IList<IChange> changes)
        {
            this.Context = contextType;
            this.Changes = changes ?? Enumerable.Empty<IChange>();
        }

        public Type Context { get; private set; }

        public IEnumerable<IChange> Changes { get; private set; }
    }
}