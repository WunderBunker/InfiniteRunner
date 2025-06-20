using TMPro;
using UnityEngine;
using UnityEngine.UI;

//INDICATEUR UI DE CHARON (BARRES DE BRUIT ET CHRONO)
public class CharonIndicator : MonoBehaviour
{

    [SerializeField] Color[] _barsColors = new Color[5];

    Color _idleColor;
    Transform _barsLayout;
    TextMeshProUGUI _chrono;
    Image _speaker;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _barsLayout = transform.Find("NoiseBars");
        _chrono = transform.Find("Chrono").GetComponent<TextMeshProUGUI>();
        _speaker = transform.Find("Speaker").GetComponent<Image>();
        _idleColor = _speaker.color;
    }

    public void MajNoiseValue(byte pValue)
    {
        for (int lCptChild = 0; lCptChild < _barsLayout.childCount; lCptChild++)
            _barsLayout.GetChild(lCptChild).GetComponent<Image>().color = lCptChild >= _barsLayout.childCount - pValue ? _barsColors[pValue - 1] : _idleColor;
        _speaker.color = pValue > 0 ? _barsColors[pValue - 1] : _idleColor;
    }

    public void MajChronoValue(float pValue)
    {
        _chrono.text = ((int)(pValue / 60)).ToString("0") + ":" + (pValue % 60).ToString("0");
    }
}