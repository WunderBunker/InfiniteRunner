using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class BulletsUI : MonoBehaviour
{

    Slider _reloadSlider;
    TextMeshProUGUI _stockText;
    byte _stockMaxValue;
    RectMask2D _fireRateMask;
    float _maskSize;

    void Start()
    {
        _reloadSlider = transform.Find("ReloadSlider").GetComponent<Slider>();
        _stockText = transform.Find("Count").GetComponent<TextMeshProUGUI>();
        _fireRateMask = transform.Find("Image").GetComponent<RectMask2D>();
        _maskSize = transform.Find("Image").GetComponent<RectTransform>().rect.height;
    }

    public void SetStockMaxValue(byte pMaxValue)
    {
        _stockMaxValue = pMaxValue;
    }

    public void SetStockValue(byte pStockValue)
    {
        _stockText.text = pStockValue + "/" + _stockMaxValue;
    }

    public void SetReloadValue(float pValue)
    {
        _reloadSlider.value = pValue;
    }

    public void SetFireRateAvancement(float pValue)
    {
        _fireRateMask.padding = new Vector4(0,math.lerp(_maskSize,0,pValue),0,0);
    }

}
