using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SliderHandler : MonoBehaviour
{
    private Slider slider;
    public AudioMixer audioMixer;
    public enum SliderType
    {
        Music,
        SFX,
        PanSpeed
    }
    public SliderType sliderType;
    void Start()
    {
        slider = GetComponent<Slider>();
    }

public void OnSliderValueChanged(float value)
{
    float dB;

    if (value < 0.5f)
    {
        float t = value / 0.5f;
        dB = Mathf.Lerp(-80f, 0f, Mathf.Pow(t, 2f)); 
    }
    else
    {
        float t = (value - 0.5f) / 0.5f; 
        dB = Mathf.Lerp(0f, 20f, t);
    }

    switch (sliderType)
    {
        case SliderType.Music:
            audioMixer.SetFloat("Music", dB);
            break;

        case SliderType.SFX:
            audioMixer.SetFloat("SFX", dB);
            break;

        case SliderType.PanSpeed:
            float panSpeed = math.lerp(5f, 15f, value);
            PlayerPrefs.SetFloat("PanSpeed", panSpeed);
            break;
    }
}


}
