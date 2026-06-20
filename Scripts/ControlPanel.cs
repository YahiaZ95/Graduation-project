using UnityEngine;

public class ControlPanel : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject PlantsType;
    public GameObject GroundSizePanel;
    public GameObject WellPosPanel;
    public GameObject DengerZonePanal;
    public GameObject SettingsPanal;

    public void TogglePanel(GameObject panel)
    {
        if (panel != null)
            panel.SetActive(!panel.activeSelf);
    }

    public void OpenPanel(GameObject panel)
    {
        if (panel == null)
            return;

        bool wasOpen = panel.activeSelf;

        CloseAllPanels();

        if (!wasOpen)
            panel.SetActive(true);
    }

    public void CloseAllPanels()
    {
        SetPanelActive(PlantsType, false);
        SetPanelActive(GroundSizePanel, false);
        SetPanelActive(WellPosPanel, false);
        SetPanelActive(DengerZonePanal, false);
        SetPanelActive(SettingsPanal, false);

    }

    private void SetPanelActive(GameObject panel, bool value)
    {
        if (panel != null)
            panel.SetActive(value);
    }

    public void ShowPlantsType() => OpenPanel(PlantsType);
    public void ShowGroundSizePanel() => OpenPanel(GroundSizePanel);
    public void ShowWellPosPanel() => OpenPanel(WellPosPanel);
    public void ShowDangerZonePanel() => OpenPanel(DengerZonePanal);
    public void ShowSettingsPanel() => OpenPanel(SettingsPanal);


    public void TogglePlantsType() => TogglePanel(PlantsType);
    public void ToggleGroundSizePanel() => TogglePanel(GroundSizePanel);
    public void ToggleWellPosPanel() => TogglePanel(WellPosPanel);
    public void ToggleDangerZonePanel() => TogglePanel(DengerZonePanal);
    public void ToggleSettingsPanel() => TogglePanel(SettingsPanal);
}

