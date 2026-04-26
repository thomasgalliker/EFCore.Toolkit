using System.Diagnostics;

namespace EFCore.Toolkit.Abstractions
{
    [DebuggerDisplay("ChangedEntity='{ChangedEntity}', State={State}", Type = "Change")]
    public class Change : IChange
    {
        private Change(object changedEntity, ChangeState state)
            : this(changedEntity, state, null!)
        {
        }

        private Change(object changedEntity, ChangeState state, IEnumerable<PropertyChangeInfo> changedProperties)
        {
            if (changedEntity == null)
            {
                throw new ArgumentNullException(nameof(changedEntity));
            }

            if (state == ChangeState.Modified && changedProperties == null)
            {
                throw new ArgumentNullException($"Parameter {nameof(changedProperties)} must be defined if ChangeState is Modified.", nameof(changedProperties));
            }

            this.ChangedEntity = changedEntity;
            this.ChangedProperties = changedProperties;
            this.State = state;
        }

        public object ChangedEntity { get; private set; }

        public IEnumerable<PropertyChangeInfo> ChangedProperties { get; private set; }

        public ChangeState State { get; private set; }

        public static IChange CreateUpdateChange(object changedEntity, IEnumerable<PropertyChangeInfo> changedProperties)
        {
            return new Change(changedEntity, ChangeState.Modified, changedProperties);
        }

        public static IChange CreateDeleteChange(object changedEntity)
        {
            return new Change(changedEntity, ChangeState.Deleted);
        }

        public static IChange CreateAddedChange(object changedEntity)
        {
            return new Change(changedEntity, ChangeState.Added);
        }
    }
}