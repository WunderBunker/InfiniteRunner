using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInBlack : MonoBehaviour
{
    [SerializeField] float _speed;
    Image _image;

    void Awake() => _image = GetComponent<Image>();

    void Start()
    {
        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 1);
        StartCoroutine(FadeOut());
    }

    public IEnumerator FadeIn()
    {
        Color vColor = _image.color;
        while (vColor.a < 1)
        {
            vColor.a += Time.deltaTime * _speed;
            _image.color = vColor;
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator FadeOut()
    {
        Color vColor = _image.color;
        while (vColor.a > 0)
        {
            vColor.a -= Time.deltaTime * _speed;
            _image.color = vColor;
            yield return new WaitForEndOfFrame();
        }

    }
}
