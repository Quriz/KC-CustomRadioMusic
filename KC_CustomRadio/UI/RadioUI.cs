using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Quriz.CustomRadio;

public class RadioUI : MonoBehaviour
{
    private CustomRadioController _radioController;

    private TMP_InputField _inputField;
    private Transform _content;
    private ToggleGroup _contentToggleGroup;
    private TextMeshProUGUI _loadingText;

    private readonly List<RadioUISongItem> _items = [];

    public static RadioUI CreateForRadio(CustomRadioController radio)
    {
        var ui = Instantiate(Assets.CustomRadioUIPrefab).AddComponent<RadioUI>();
        ui.gameObject.SetActive(false);
        ui._radioController = radio;
        return ui;
    }

    private void Awake()
    {
        _inputField = GetComponentInChildren<TMP_InputField>();
        _inputField.onEndEdit.AddListener(AddSongs);

        var viewport = GetComponentInChildren<ScrollRect>().viewport;
        
        _loadingText = viewport.GetComponentInChildren<TextMeshProUGUI>();
        _loadingText.gameObject.SetActive(false);
        
        _contentToggleGroup = viewport.GetComponentInChildren<ToggleGroup>();
        _content = _contentToggleGroup.transform;
        
        GetComponentInChildren<Button>().onClick.AddListener(Close);
    }

    public void SetDownloading(bool isDownloading)
    {
        _loadingText.gameObject.SetActive(isDownloading);
    }
    
    public void SetLoadingText(string text)
    {
        _loadingText.text = text;
    }

    private void AddSongs(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;
        
        _radioController.AddSongs(_inputField.text.Trim());
        _inputField.text = "";
    }

    public void Open()
    {
        if (GameServer.LocalItemInteract is CharacterItemInteract localItemInteract
            && localItemInteract.GetInterface() is CharacterInterface characterInterface)
        {
            characterInterface.OpenMenu(gameObject, true);
            characterInterface.HideCrosshairOverlay();
        }
    }

    public void Close()
    {
        if (GameServer.LocalItemInteract is CharacterItemInteract localItemInteract
            && localItemInteract.GetInterface() is CharacterInterface characterInterface)
        {
            characterInterface.OpenMenu(gameObject);
        }
    }

    public void UpdatePlaylist(List<string> playlist)
    {
        // Remove old items
        var itemsToRemove = _items.Where(item => !playlist.Contains(item.fileName)).ToList();
        foreach (var item in itemsToRemove)
        {
            _items.Remove(item);
            Destroy(item.gameObject);
        }
        
        // Add new items
        var songsToAdd = playlist.Where(song => _items.All(item => item.fileName != song)).ToList();
        foreach (var songFileName in songsToAdd)
        {
            var item = Instantiate(Assets.SongItemPrefab, _content).AddComponent<RadioUISongItem>();
            item.Init(_radioController, songFileName, _contentToggleGroup);
            
            _items.Add(item);
        }
    }
}