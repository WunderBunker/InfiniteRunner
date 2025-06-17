using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ATTAQUE PREMIERE DE SCYLLA (PETIT SERPENT)
public class ScyllaAttack2 : MonoBehaviour
{
    [SerializeField] GameObject _preAttack;
    [SerializeField] float _preAttackTime;
    [SerializeField] GameObject _attack;
    [SerializeField] float _attackTime;
    [SerializeField] float _attackAnimTime;
    [SerializeField] GameObject _splash;
    [SerializeField] List<AudioClip> _splashSounds = new();
    [NonSerialized] public Color AttackColor;
    float _attackTimer;

    Transform _playerTransform;
    AttackState _state;
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

        _state = AttackState.preparing;
        _attackTimer = _preAttackTime;

        _PS = transform.Find("PreAttack").GetComponent<ParticleSystem>();

        transform.position = new Vector3(transform.position.x, transform.position.y, _playerTransform.position.z);
        _preAttack.SetActive(true);
        _PS.Play();
    }

    void Update()
    {
        _attackTimer -= Time.deltaTime;

        //maj de l'état de l'attaque
        if (_state == AttackState.preparing && _attackTimer <= 0)
        {
            _state = AttackState.attacking;
            _snakeAnimator.SetTrigger("Jump");
            _attack.SetActive(true);
            transform.Find("Snake").Find("Skin").GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_Emission", AttackColor);
            Instantiate(_splash, transform.Find("SplashPosition").position, _splash.transform.rotation, transform.parent);
            AudioManager.Instance.PlaySound(_splashSounds[new System.Random().Next(0, _splashSounds.Count)], 1);
            _attackTimer = _attackTime;
            _PS.Stop();
            StartCoroutine(WaitEndAnim());
        }
        else if (_state == AttackState.attacking)
        {
            if (_attack.activeSelf && _attackTimer <= 0)
            {
                _attack.SetActive(false);
                transform.Find("Snake").Find("Skin").GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_Emission", new Color(1, 1, 1, 0.1f));
            }
        }

        //On divise l'attackRange par deux car le pivot est au milieu
        transform.position = new Vector3(transform.position.x, transform.position.y, _playerTransform.position.z);
    }

    //On attend la fin de l'anim en cours avant de dtruire l'objet
    IEnumerator WaitEndAnim()
    {
        yield return new WaitForSeconds(_attackAnimTime);
        Instantiate(_splash, transform.Find("SplashPosition").position, _splash.transform.rotation, transform.parent);
        AudioManager.Instance.PlaySound(_splashSounds[new System.Random().Next(0, _splashSounds.Count)], 1);
        Destroy(gameObject);
    }
}

