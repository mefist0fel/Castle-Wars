#if UNITY_EDITOR
using System;

namespace Configurations.Editor.Drawers
{
    public abstract class AbstractDrawer<T> : AbstractDrawer
    {
        public sealed override bool IsTypeSupported(Type type)
            => type == typeof(T);
        public sealed override void ShowDrawerGUI(object obj, Type type, string name, IValueSetter setter, int instanceId)
            => ShowDrawerGUI((T)obj, name, setter, instanceId);

        public abstract void ShowDrawerGUI(T obj, string name, IValueSetter setter, int instanceId);
    }

    public abstract class AbstractDrawer
    {
        public abstract bool IsTypeSupported(Type type);

        public abstract void ShowDrawerGUI(object obj, Type type, string name, IValueSetter setter, int instanceId);
    }
}
#endif