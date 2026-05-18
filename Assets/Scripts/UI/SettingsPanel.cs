using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _blurBackground;
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private Button _closeButton;

    [Header("Audio")]
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private string _volumeParam = "MasterVolume";

    private const string VolumeKey = "MasterVolume";

    private void OnEnable()
    {
        _closeButton.onClick.AddListener(Close);

        _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        float saved =
            PlayerPrefs.GetFloat(
                VolumeKey,
                1f);

        _volumeSlider.SetValueWithoutNotify(saved);

        ApplyVolume(saved);

        if (_blurBackground != null)
        {
            _blurBackground.SetActive(true);
        }
    }

    private void OnDisable()
    {
        _closeButton.onClick.RemoveListener(Close);

        _volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }

    private void OnVolumeChanged(float value)
    {
        ApplyVolume(value);

        PlayerPrefs.SetFloat(
            VolumeKey,
            value);
    }

    private void ApplyVolume(float value)
    {
        float dB =
            value > 0.001f
            ? Mathf.Log10(value) * 20f
            : -80f;

        _audioMixer.SetFloat(
            _volumeParam,
            dB);
    }

    public void Open()
    {
        if (_blurBackground != null)
        {
            _blurBackground.SetActive(true);
        }

        gameObject.SetActive(true);
    }

    public void Close()
    {
        if (_blurBackground != null)
        {
            _blurBackground.SetActive(false);
        }

        gameObject.SetActive(false);
    }
}