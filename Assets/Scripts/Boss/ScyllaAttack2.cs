using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScyllaAttack2 : MonoBehaviour
{
    [SerializeField] GameObject _preAttack;
    [SerializeField] float _preAttackTime;
    [SerializeField] GameObject _attack;
    [SerializeField] float _attackTime;
    [SerializeField] float _attackAnimTime;
    [SerializeField] GameObject _splash;
    [SerializeField] List<AudioClip> _splashSounds = new();
    float _attackTimer;

    Transform _playerTransform;
    ScyllaAttackState _state;
    Animator _snakeAnimator;
    ParticleSystem _PS;

    public float GetAttackRange()
    {
        return _attack.GetComponent<BoxCollider>().size.z * _attack.transform.localScale.z;
    }

    void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _snakeAnimator = transform.Find("Snake").GetComponent<Animator>();

        _state = ScyllaAttackState.preparing;
        _attackTimer = _preAttackTime;

        _PS = transform.Find("PreAttack").GetComponent<ParticleSystem>();

        transform.position = new Vector3(transform.position.x, transform.position.y, _playerTransform.position.z);
        _preAttack.SetActive(true);
        _PS.Play();
    }

    void Update()
    {
        _attackTimer -= Time.deltaTime;

        if (_state == ScyllaAttackState.preparing && _attackTimer <= 0)
        {
            _state = ScyllaAttackState.attacking;
            _snakeAnimator.SetTrigger("Jump");
            _attack.SetActive(true);
            transform.Find("Snake").Find("Skin").GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_Emission", new Color(1, 0, 1, 0.9f));
            Instantiate(_splash, transform.Find("SplashPosition").position, _splash.transform.rotation, transform.parent);
            AudioManager.Instance.PlaySound(_splashSounds[new System.Random().Next(0, _splashSounds.Count)], 1);
            _attackTimer = _attackTime;
            _PS.Stop();
            StartCoroutine(WaitEndAnim());
        }
        else if (_state == ScyllaAttackState.attacking)
        {
            if (_attack.activeSelf && _attackTimer <= 0)
            {
                _attack.SetActive(false);
            transform.Find("Snake").Find("Skin").GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_Emission", new Color(1, 1, 1, 0.1f));}
        }

        //On divise l'attackRange par deux car le pivot est au milieu
        transform.position = new Vector3(transform.position.x, transform.position.y, _playerTransform.position.z);
    }

    IEnumerator WaitEndAnim()
    {
        yield return new WaitForSeconds(_attackAnimTime);
        Instantiate(_splash, transform.Find("SplashPosition").position, _splash.transform.rotation, transform.parent);
        AudioManager.Instance.PlaySound(_splashSounds[new System.Random().Next(0, _splashSounds.Count)], 1);
        Destroy(gameObject);
    }
}

