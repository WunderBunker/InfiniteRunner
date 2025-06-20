using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//GESTION DU FILTRE UI DES BOSS (WARNING, ALARM)
public class BossFilter : MonoBehaviour
{
    [SerializeField] float _maxAlpha;
    [SerializeField] float _speed = 1;
    [SerializeField] AudioClip _alertSound;
    [SerializeField] bool _activeOnStart;
    [SerializeField] float _maxTime;
    [SerializeField] float _soundDelay = 0;
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
        //Alpha en cours d'animation
        if (_isAnim)
        {
            //On va vers l'apparition du filtre
            if (_onAlert)
            {
                Color vColor = _image.color;
                vColor.a += Time.deltaTime * _speed;
                if (vColor.a >= _maxAlpha)
                {
                    vColor.a = _maxAlpha;
                    _isAnim = false;
                    _timer = _maxTime;
                }
                _image.color = vColor;
            }
            //On va vers la disparition du filtre
            else
            {
                Color vColor = _image.color;
                vColor.a -= Time.deltaTime * _speed;
                if (vColor.a <= 0)
                {
                    vColor.a = 0;
                    _isAnim = false;
                }
                _image.color = vColor;
            }
        }


        //Gestion d'un éventuel timer pour disparition auto
        if (_timer > 0 && _maxTime > 0)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                SetActive(false);
            }
        }
    }

    //Affichage/désaffichage du filtre
    public void SetActive(bool pActive)
    {
        _onAlert = pActive;
        if (pActive && _alertSound != null) StartCoroutine(PlayWarningSound());
        _isAnim = true;
    }

    IEnumerator PlayWarningSound()
    {
        yield return new WaitForSeconds(_soundDelay);
        AudioManager.Instance.PlaySound(_alertSound, 0.9f);
    }
}
