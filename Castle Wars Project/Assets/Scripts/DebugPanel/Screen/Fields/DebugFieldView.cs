using System;
using UnityEngine;

// Abstract base for all typed debug input-field prefabs.
//
// Used in two contexts:
//   DebugActionBlock -- GetValue() called on button click (read-only from UI perspective)
//   DebugVariableBlock -- SetDisplayValue() polled each LateUpdate; OnValueCommitted wired to setter
//
// Concrete subclasses: DebugFieldViewInt, Float, String, Bool, Enum.
public abstract class DebugFieldView : MonoBehaviour
{
    // Subscribed by DebugVariableBlock to write committed user input back to the variable.
    // Null in action-param mode -- implementations must not assume it is set.
    public Action<object> OnValueCommitted;

    // Initializes the control: applies content constraints and displays the default value.
    public abstract void Initialize(DebugParam param);

    // Returns the current user-entered value as a boxed object.
    public abstract object GetValue();

    // Pushes a value into the control for display (variable sync, called each frame).
    // Must NOT fire OnValueCommitted when updating display programmatically.
    public abstract void SetDisplayValue(object value);
}
