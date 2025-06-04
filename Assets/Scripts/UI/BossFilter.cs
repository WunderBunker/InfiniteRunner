using UnityEngine;
using UnityEngine.UI;

public class BossFilter : MonoBehaviour
{
    [SerializeField] float _maxAlpha;
    [SerializeField] AudioClip _alertSound;
    [SerializeField] bool _activeOnStart;
    [SerializeField] float _maxTime;
    bool _onAlert;
    bool _isAnim;
    Image _image;
    float _timer;

    void Awake()
    {
        _image = GetComponent<Image>();
    }

    void Start()
    {
        if (_activeOnStart) SetActive(true);
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
                    _timer = _maxTime;
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

        if (_timer > 0 && _maxTime > 0 )
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                SetActive(false);
            }
        }
    }

    public void SetActive(bool pActive)
    {
        _onAlert = pActive;
        if (pActive && _alertSound!=null) AudioManager.Instance.PlaySound(_alertSound, 0.8f);
        _isAnim = true;
    }
}
