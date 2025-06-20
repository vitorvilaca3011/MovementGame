using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown monitorDropdown;
    public Toggle fullscreenToggle;
    public Slider sensitivityXSlider;
    public Slider sensitivityYSlider;
    public Toggle dynamicFOVToggle;

    private Resolution[] resolutions;

    void Start()
    {
        // Populate resolutions
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentResIndex = 0;
        var options = new System.Collections.Generic.List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();

        // Load saved sensitivity
        sensitivityXSlider.onValueChanged.AddListener(UpdateSensitivityX);
        sensitivityYSlider.onValueChanged.AddListener(UpdateSensitivityY);

        sensitivityXSlider.value = PlayerPrefs.GetFloat("MouseSensitivityX", 15f);
        sensitivityYSlider.value = PlayerPrefs.GetFloat("MouseSensitivityY", 15f);

        // Load FOV setting
        dynamicFOVToggle.isOn = PlayerPrefs.GetInt("DynamicFOV", 1) == 1;

        // Load fullscreen setting
        fullscreenToggle.isOn = Screen.fullScreen;
    }

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetSensitivityX(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivityX", value);
    }

    public void SetSensitivityY(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivityY", value);
    }

    public void UpdateSensitivityX(float value)
    {
        PlayerPrefs.SetFloat("SensitivityX", value);
        PlayerPrefs.Save();
    }

    public void UpdateSensitivityY(float value)
    {
        PlayerPrefs.SetFloat("SensitivityY", value);
        PlayerPrefs.Save();
    }

    public void SetDynamicFOV(bool value)
    {
        PlayerPrefs.SetInt("DynamicFOV", value ? 1 : 0);
    }

    public void SetMonitor(int monitorIndex)
    {
        if (monitorIndex < Display.displays.Length)
        {
            Display.displays[monitorIndex].Activate();
        }
    }

    public void BackToMainMenu(GameObject optionsMenu, GameObject mainMenu)
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
}
