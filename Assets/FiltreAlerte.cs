using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class FiltreAlerte : MonoBehaviour
{
    [SerializeField] float _maxAlpha;
    [SerializeField] AudioClip _alertSound;
    bool _onAlert;
    bool _isAnim;
    float _timer;
    Image _image;

    void Awake()
    {
        _image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {

        if (_isAnim)
        {
            if (_onAlert)
            {
                Color vColor = _image.color;
                vColor.a += Time.deltaTime;
                if (vColor.a >= _maxAlpha)
                {
                    vColor.a = _maxAlpha;
                    _isAnim = false;
                }
                _image.color = vColor;
            }
            else
            {
                Color vColor = _image.color;
                vColor.a -= Time.deltaTime;
                if (vColor.a <= 0)
                {
                    vColor.a = 0;
                    _isAnim = false;
                }
                _image.color = vColor;
            }
        }

    }

    public void SetActive(bool pActive)
    {
        _onAlert = pActive;
        if (pActive) AudioManager.Instance.PlaySound(_alertSound, 0.8f);
        _isAnim = true;
    }
}
