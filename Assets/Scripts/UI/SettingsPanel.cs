using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RawImage _blurBackground;
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private Button _closeButton;

    [Header("Audio")]
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private string _volumeParam = "MasterVolume";

    [Header("Blur")]
    [SerializeField] private int _blurIterations = 4;
    [SerializeField] private float _blurSize = 2f;

    private const string VolumeKey = "MasterVolume";
    private Texture2D _blurTexture;

    private void OnEnable()
    {
        _closeButton.onClick.AddListener(Close);
        _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        float saved = PlayerPrefs.GetFloat(VolumeKey, 1f);
        _volumeSlider.SetValueWithoutNotify(saved);
        ApplyVolume(saved);

        StartCoroutine(CaptureAndBlur());
    }

    private void OnDisable()
    {
        _closeButton.onClick.RemoveListener(Close);
        _volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);

        if (_blurTexture != null)
        {
            Destroy(_blurTexture);
            _blurTexture = null;
        }
    }

    private IEnumerator CaptureAndBlur()
    {
        // Скрываем панель чтобы скриншот был чистым
        _blurBackground.gameObject.SetActive(false);
        yield return new WaitForEndOfFrame();

        // Делаем скриншот
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();

        // Размываем
        _blurTexture = BlurTexture(screenshot, _blurIterations, _blurSize);
        Destroy(screenshot);

        // Показываем
        _blurBackground.texture = _blurTexture;
        _blurBackground.gameObject.SetActive(true);
    }

    private Texture2D BlurTexture(Texture2D source, int iterations, float blurSize)
    {
        int width = source.width;
        int height = source.height;

        Color[] pixels = source.GetPixels();
        Color[] result = new Color[pixels.Length];

        for (int iter = 0; iter < iterations; iter++)
        {
            // Горизонтальный проход
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color sum = Color.clear;
                    float weightSum = 0f;
                    int radius = Mathf.RoundToInt(blurSize);

                    for (int i = -radius; i <= radius; i++)
                    {
                        int nx = Mathf.Clamp(x + i, 0, width - 1);
                        float weight = Mathf.Exp(-(i * i) / (2f * blurSize * blurSize));
                        sum += pixels[y * width + nx] * weight;
                        weightSum += weight;
                    }

                    result[y * width + x] = sum / weightSum;
                }
            }

            // Копируем результат обратно для вертикального прохода
            System.Array.Copy(result, pixels, pixels.Length);

            // Вертикальный проход
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color sum = Color.clear;
                    float weightSum = 0f;
                    int radius = Mathf.RoundToInt(blurSize);

                    for (int i = -radius; i <= radius; i++)
                    {
                        int ny = Mathf.Clamp(y + i, 0, height - 1);
                        float weight = Mathf.Exp(-(i * i) / (2f * blurSize * blurSize));
                        sum += pixels[ny * width + x] * weight;
                        weightSum += weight;
                    }

                    result[y * width + x] = sum / weightSum;
                }
            }

            System.Array.Copy(result, pixels, pixels.Length);
        }

        Texture2D blurred = new Texture2D(width, height, TextureFormat.RGB24, false);
        blurred.SetPixels(pixels);
        blurred.Apply();
        return blurred;
    }

    private void OnVolumeChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat(VolumeKey, value);
    }

    private void ApplyVolume(float value)
    {
        float dB = value > 0.001f ? Mathf.Log10(value) * 20f : -80f;
        _audioMixer.SetFloat(_volumeParam, dB);
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}