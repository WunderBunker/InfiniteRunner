using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScyllaAttack : MonoBehaviour
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
    float _attackRange;
    AttackState _state;
    Animator _snakeAnimator;

    public float GetAttackRange()
    {
        return _attack.GetComponent<BoxCollider>().size.z * _attack.transform.localScale.z;
    }

    void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _attackRange = GetAttackRange();
        _snakeAnimator = transform.Find("Snake").GetComponent<Animator>();

        _state = AttackState.preparing;
        _attackTimer = _preAttackTime -_attackAnimTime;
        //On divise l'attackRange par deux car le pivot est au milieu
        transform.position = new Vector3(transform.position.x, transform.position.y, _playerTransform.position.z - _attackRange / 2 + 5);
        _preAttack.SetActive(true);
        _snakeAnimator.SetTrigger("Prepare");
    }


    void Update()
    {
        _attackTimer -= Time.deltaTime;

        if (_attackTimer <= 0)
            switch (_state)
            {
                case AttackState.preparing:
                    _state = AttackState.attacking;
                    _snakeAnimator.SetTrigger("Attack");
                    //On met le vrai timer à la fin de coroutine
                    _attackTimer =1000;
                    StartCoroutine(WaitEndAnim());
                    break;
                case AttackState.attacking:
                    Destroy(gameObject);
                    return;
                default:
                    break;
            }

        //On divise l'attackRange par deux car le pivot est au milieu
        transform.position = new Vector3(transform.position.x, transform.position.y, _playerTransform.position.z - _attackRange / 2 + 5);
    }


    IEnumerator WaitEndAnim()
    {
        yield return new WaitForSeconds(_attackAnimTime);
        _preAttack.SetActive(false);
        _attack.SetActive(true);
        transform.Find("Snake").Find("Skin").GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_Emission", new Color(1, 0, 1, 0.9f));
        
        _attackTimer = _attackTime;
        Instantiate(_splash, transform.Find("SplashPosition").position, _splash.transform.rotation, transform.parent);
        AudioManager.Instance.PlaySound(_splashSounds[new System.Random().Next(0, _splashSounds.Count)],1);
    }
}

