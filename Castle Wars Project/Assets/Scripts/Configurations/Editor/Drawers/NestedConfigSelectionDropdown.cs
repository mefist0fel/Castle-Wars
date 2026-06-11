#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public sealed class NestedConfigSelectionDropdown : AdvancedDropdown
{
    private readonly System.Action<int> _onSelected;
    private readonly List<string> _options;

    public NestedConfigSelectionDropdown(AdvancedDropdownState state, List<string> options, System.Action<int> onSelected)
        : base(state)
    {
        _options = options;
        _onSelected = onSelected;
        // Set the minimum size for the window
        minimumSize = new Vector2(200, 300);
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        // The root item is hidden, its name doesn't matter much
        var root = new AdvancedDropdownItem("Configurations");

        for (int i = 0; i < _options.Count; i++)
        {
            string path = _options[i];

            // Handle null or empty names
            if (string.IsNullOrEmpty(path)) path = "None (null)";

            // Split path by forward slash
            string[] parts = path.Split('/');

            AdvancedDropdownItem parent = root;

            // Iterate through all parts except the last one to create folders
            for (int j = 0; j < parts.Length - 1; j++)
            {
                string folderName = parts[j];
                var existingFolder = parent.children.FirstOrDefault(child => child.name == folderName);

                if (existingFolder != null)
                {
                    parent = existingFolder;
                }
                else
                {
                    var newFolder = new AdvancedDropdownItem(folderName);
                    parent.AddChild(newFolder);
                    parent = newFolder;
                }
            }

            // Create the final leaf item
            // We use the full path or ID in some way to identify it later, 
            // but for simple IndexOf, name is fine if they are unique.
            var item = new AdvancedDropdownItem(parts.Last());

            parent.AddChild(item);
            item.id = i; // Store the original index in the id field for easy retrieval
        }

        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        base.ItemSelected(item);
        _onSelected?.Invoke(item.id);
    }
}
#endif