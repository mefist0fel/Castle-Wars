using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sound
{
    /// <summary>
    /// Attach to any GameObject that has a Button component.
    /// Subscribes to Button.onClick and plays the configured sound through the library.
    /// When added or reset in the editor, automatically finds the first SoundLibrary asset
    /// in the project and assigns it to <see cref="_library"/>.
    /// </summary>
    [AddComponentMenu("Sound/Sound Button Player")]
    [RequireComponent(typeof(Button))]
    public sealed class ButtonSoundPlayer : MonoBehaviour
    {
        [SerializeField] private SoundLibrary _library;
        [SerializeField, SoundId] private string _soundId;

#if UNITY_EDITOR
        private void Reset()
        {
            if (_library != null)
                return;

            var guids = AssetDatabase.FindAssets("t:SoundLibrary");
            if (guids.Length == 0)
                return;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _library = AssetDatabase.LoadAssetAtPath<SoundLibrary>(path);
        }
#endif

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(PlaySound);
        }

        private void OnDestroy()
        {
            if (TryGetComponent<Button>(out var button))
                button.onClick.RemoveListener(PlaySound);
        }

        private void PlaySound()
        {
            if (_library == null || string.IsNullOrEmpty(_soundId))
                return;

            _library.Play(_soundId);
        }
    }
}
