using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharonAttack2 : MonoBehaviour
{
    [SerializeField] GameObject _preAttack;
    [SerializeField] float _preAttackTime;
    [SerializeField] GameObject _attack;
    [SerializeField] float _attackTime;
    float _attackTimer;

    Transform _playerTransform;
    AttackState _state;
    Animator _handsAnimator;
    ParticleSystem _PS;

    public float GetAttackRange()
    {
        return _attack.GetComponent<BoxCollider>().size.z * _attack.transform.localScale.z;
    }

    void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _handsAnimator = transform.Find("Hands").GetComponent<Animator>();

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

        if (_state == AttackState.preparing && _attackTimer <= 0)
        {
            _state = AttackState.attacking;
            _handsAnimator.SetTrigger("Rise");

            _PS.Stop();
            StartCoroutine(WaitEndAnim());
        }
        else if (_state == AttackState.attacking)
            if (_attack.activeSelf && _attackTimer <= 0)
                _attack.SetActive(false);

        //On divise l'attackRange par deux car le pivot est au milieu
        transform.position = new Vector3(transform.position.x, transform.position.y, _playerTransform.position.z);
    }

    IEnumerator WaitEndAnim()
    {
        yield return new WaitForSeconds(0.25f);
        _attack.SetActive(true);
        _attackTimer = _attackTime;
        yield return new WaitForSeconds(_handsAnimator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }
}