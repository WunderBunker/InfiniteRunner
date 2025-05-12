using UnityEngine;
using UnityEngine.UI;

public class BossIndicator : MonoBehaviour
{
    [SerializeField] GameObject _bossObject;
    IBoss _boss;
    Slider _slider;

    void Start()
    {
        _slider = GetComponent<Slider>();
        _slider.value = 0;
        _boss = (IBoss)_bossObject.GetComponent(typeof(IBoss));
    }

    // Update is called once per frame
    void Update()
    {
        _slider.value = _boss.DistanceToPlayerRatio;
    }
}
