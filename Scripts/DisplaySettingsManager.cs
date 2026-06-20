using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class DisplaySettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    struct ResolutionItem
    {
        public int width;
        public int height;

        public ResolutionItem(int w, int h)
        {
            width = w;
            height = h;
        }
    }

    List<ResolutionItem> resolutions = new List<ResolutionItem>()
    {
        new ResolutionItem(1920, 1200),
        new ResolutionItem(1600, 1200),
        new ResolutionItem(1280, 1024),
        new ResolutionItem(1280, 800),
        new ResolutionItem(800, 600)
    };

    void Start()
    {
        SetupDropdown();
        LoadSettings();
    }

    void SetupDropdown()
    {
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        foreach (var res in resolutions)
        {
            options.Add(res.width + " x " + res.height);
        }

        resolutionDropdown.AddOptions(options);

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    void SetResolution(int index)
    {
        var res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);

        PlayerPrefs.SetInt("resIndex", index);
    }

    void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;

        PlayerPrefs.SetInt("fullscreen", isFullscreen ? 1 : 0);
    }

    void LoadSettings()
    {
        int savedIndex = PlayerPrefs.GetInt("resIndex", 0);
        int savedFullscreen = PlayerPrefs.GetInt("fullscreen", 1);

        resolutionDropdown.value = savedIndex;
        fullscreenToggle.isOn = savedFullscreen == 1;

        SetResolution(savedIndex);
        SetFullscreen(savedFullscreen == 1);
    }
}