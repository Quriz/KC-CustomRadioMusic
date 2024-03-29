using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Quriz.CustomRadio;

public class RadioUISongItem : MonoBehaviour
{
    private static readonly Color SelectedColor = new Color(0.85f, 0.67f, 0.21f);
    
    public string fileName;

    public void Init(CustomRadioController radioController, string fileName, ToggleGroup toggleGroup)
    {
        this.fileName = fileName;

        // Label
        var label = GetComponentInChildren<TextMeshProUGUI>();
        label.text = Path.GetFileNameWithoutExtension(fileName);
        
        // Delete button
        GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            radioController.RemoveSongFromPlaylist(fileName);
        });
        
        // Toggle click
        var toggle = GetComponentInChildren<Toggle>();
        toggle.group = toggleGroup;
        toggle.onValueChanged.AddListener(isChecked =>
        {
            label.color = isChecked ? SelectedColor : Color.white;
            if (isChecked)
                radioController.Play(fileName);
            else
                radioController.PausePlaying(fileName);
        });

        // Update UI when song automatically changes
        radioController.OnSongChanged += (sender, songFileName) =>
        {
            var isThisItemEnabled = songFileName == fileName;
            label.color = isThisItemEnabled ? SelectedColor : Color.white;
            toggle.SetIsOnWithoutNotify(isThisItemEnabled);
        };
    }
}