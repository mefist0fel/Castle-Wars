#if UNITY_EDITOR
using System.Reflection;

namespace Configurations.Editor.Drawers
{
    internal sealed class StructFieldSetter : IValueSetter
    {
        private FieldInfo _field;
        private object _targetStructure; // Boxed copy
        private IValueSetter _parentSetter;

        public void Set(object targetStructure, FieldInfo field, IValueSetter parentSetter)
        {
            _targetStructure = targetStructure;
            _field = field;
            _parentSetter = parentSetter;
        }

        public void SetValue(object value)
        {
            _field.SetValue(_targetStructure, value);
            _parentSetter.SetValue(_targetStructure);
        }

        public IValueSetter Clone() =>
            new StructFieldSetter()
            {
                _field = _field,
                _targetStructure = _targetStructure,
                _parentSetter = _parentSetter
            };
    }
}
#endif