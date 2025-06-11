using UnityEngine;
using UnityEngine.UI;

public class LifeUI : MonoBehaviour
{
    [SerializeField] Color _deadLifeColor;
    byte _maxLife;

    public void SetMaxLife(byte pMaxLife)
    {
        _maxLife = pMaxLife;
        GameObject vLifeUnit = transform.Find("LifeUnit").gameObject;
        for (int lCptLife = 0; lCptLife < pMaxLife; lCptLife++)
            Instantiate(vLifeUnit, transform);
        Destroy(vLifeUnit);
    }

    public void SetLifeNb(byte pLife)
    {
        for (int lCptChild = 0; lCptChild < transform.childCount; lCptChild++)
            if (lCptChild < _maxLife - pLife ) transform.GetChild(lCptChild).GetComponent<Image>().color = _deadLifeColor;
            else transform.GetChild(lCptChild).GetComponent<Image>().color = Color.white;
    }
}
