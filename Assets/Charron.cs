using UnityEngine;

public class Charron : MonoBehaviour, IBoss
{

    bool _isActive = false;
    GameObject _body;
    Transform _playerTransform;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _body = transform.GetChild(0).gameObject;
        _body.SetActive(false);
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        _body.transform.position = new Vector3(_body.transform.position.x, _body.transform.position.y, _playerTransform.position.z  - 20);
    }

    public void Activate()
    {
        if (_isActive) return;
        _isActive = true;
        _body.SetActive(true);

        GameObject.FindGameObjectWithTag("PatternsManager").GetComponent<PatternsManager>().SwitchBossMode(true);
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowPlayer>().ChangeDirection();

    }
    public void DeActivate()
    {
        if (!_isActive) return;
        _isActive = false;
        _body.SetActive(false);

        GameObject.FindGameObjectWithTag("PatternsManager").GetComponent<PatternsManager>().SwitchBossMode(false);
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowPlayer>().ChangeDirection();
    }
}
