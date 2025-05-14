using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BulletsUI : MonoBehaviour
{

    Slider _reloadSlider;
    TextMeshProUGUI _stockText;
    byte _stockMaxValue;

    void Start()
    {
        _reloadSlider = transform.Find("ReloadSlider").GetComponent<Slider>();
        _stockText = transform.Find("Count").GetComponent<TextMeshProUGUI>();
    }

    public void SetStockMaxValue(byte pMaxValue)
    {
        _stockMaxValue = pMaxValue;
    }

    public void SetStockValue(byte pStockValue)
    {
        _stockText.text = "Bullets : " + pStockValue + "/" + _stockMaxValue;
    }

    public void SetReloadValue(float pValue)
    {
        _reloadSlider.value = pValue;
    }

}
