#if UNITY_EDITOR
namespace Configurations.Editor.Drawers
{
    public interface IValueSetter
    {
        void SetValue(object value);
        IValueSetter Clone();
    }
}
#endif