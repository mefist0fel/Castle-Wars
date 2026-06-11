using UnityEngine;

namespace Sound
{
    /// <summary>
    /// Marks a string field as a sound ID.
    /// The SoundIdDrawer will render a dropdown button that lets you pick
    /// from IDs registered in any SoundLibrary asset found in the project.
    /// </summary>
    public class SoundIdAttribute : PropertyAttribute { }
}
