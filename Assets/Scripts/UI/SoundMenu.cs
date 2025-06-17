using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

//GESTION DU MENU DES OPTIONS SONORES
public class SoundMenu : MonoBehaviour
{
    [SerializeField] protected AudioMixer _mixer;

    void OnEnable()
    {
        _mixer.GetFloat("MasterVolume", out float vMasterVol);
        transform.Find("MainVolume").GetComponent<Slider>().value = Mathf.Pow(10, vMasterVol / 20);
        _mixer.GetFloat("FXVolume", out float vFXVol);
        transform.Find("FxVolume").GetComponent<Slider>().value = Mathf.Pow(10, vFXVol / 20);
        _mixer.GetFloat("MusicVolume", out float vMusicVol);
        transform.Find("MusicVolume").GetComponent<Slider>().value = Mathf.Pow(10, vMusicVol / 20);
    }

    public void OnClose()
    {
        AudioManager.Instance.PlayClickSound();
        gameObject.SetActive(false);
    }

    public void OnMasterChange(float pValue)
    {
        if (pValue > 0)
        {
            float vVolumeDB = Mathf.Log10(pValue) * 20;
            _mixer.SetFloat("MasterVolume", vVolumeDB);
        }
        else
            _mixer.SetFloat("MasterVolume", -80);
    }
    public void OnFxChange(float pValue)
    {
        if (pValue > 0)
        {
            float vVolumeDB = Mathf.Log10(pValue) * 20;
            _mixer.SetFloat("FXVolume", vVolumeDB);
        }
        else
            _mixer.SetFloat("FXVolume", -80);
    }
    public void OnMusicChange(float pValue)
    {
        if (pValue > 0)
        {
            float vVolumeDB = Mathf.Log10(pValue) * 20;
            _mixer.SetFloat("MusicVolume", vVolumeDB);
        }
        else
            _mixer.SetFloat("MusicVolume", -80);
    }

}
