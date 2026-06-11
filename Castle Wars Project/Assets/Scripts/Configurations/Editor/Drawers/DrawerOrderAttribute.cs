#if UNITY_EDITOR
using System;

namespace Configurations.Editor.Drawers
{
    /// <summary>
    /// Controls the priority of an <see cref="AbstractDrawer"/> in the type-matching list.
    /// Lower order is checked first. Drawers without this attribute default to order 500.
    /// <see cref="ClassDrawer"/> is always appended last regardless of order.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class DrawerOrderAttribute : Attribute
    {
        public int Order { get; }

        public DrawerOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
#endif
