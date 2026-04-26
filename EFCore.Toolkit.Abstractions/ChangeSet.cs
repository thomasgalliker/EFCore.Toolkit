using System.Diagnostics;

namespace EFCore.Toolkit.Abstractions
{
    [DebuggerDisplay("Context='{this.Context.Name}', Changes={this.Changes.Length}", Type = "ChangeSet")]
    public class ChangeSet
    {
        public ChangeSet(Type contextType, IChange[] changes)
        {
            this.Context = contextType;
            this.Changes = changes ?? Array.Empty<IChange>();
        }

        public Type Context { get; private set; }

        public IChange[] Changes { get; private set; }
    }
}