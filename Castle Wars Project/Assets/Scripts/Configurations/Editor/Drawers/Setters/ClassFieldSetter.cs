#if UNITY_EDITOR
using System.Reflection;

namespace Configurations.Editor.Drawers
{
    internal sealed class ClassFieldSetter : IValueSetter
    {
        internal FieldInfo field;
        internal object targetObject;
        public void Set(object newTargetObject, FieldInfo newField)
        {
            field = newField;
            targetObject = newTargetObject;
        }

        public void SetValue(object value)
        {
            field.SetValue(targetObject, value);
        }

        public IValueSetter Clone() {
            return (ClassFieldSetter)MemberwiseClone();
        }
    }
}
#endif