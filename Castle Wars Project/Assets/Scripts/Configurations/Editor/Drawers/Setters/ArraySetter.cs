#if UNITY_EDITOR
using System;

namespace Configurations.Editor.Drawers
{
    internal class ArraySetter : IValueSetter
    {
        internal Array targetArray;
        internal int idx;
        public void Set(Array target, int id)
        {
            targetArray = target;
            idx = id;
        }

        public void SetValue(object value)
        {
            targetArray.SetValue(value, idx);
        }

        public IValueSetter Clone() =>
            new ArraySetter()
            {
                targetArray = targetArray,
                idx = idx
            };
    }
}
#endif