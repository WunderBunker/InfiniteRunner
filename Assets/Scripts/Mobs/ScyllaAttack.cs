using System.Collections;
using UnityEngine;

public class ScyllaAttack : MonoBehaviour
{
    [SerializeField] GameObject _preAttack;
    [SerializeField] float _preAttackTime;
    [SerializeField] GameObject _attack;
    [SerializeField] float _attackTime;
    [SerializeField] float _attackAnimTime;
    float _attackTimer;

    Transform _playerTransform;
    float _attackRange;
    ScyllaAttackState _state;
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

        _state = ScyllaAttackState.preparing;
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
                case ScyllaAttackState.preparing:
                    _state = ScyllaAttackState.attacking;
                    _snakeAnimator.SetTrigger("Attack");
                    //On met le vrai timer à la fin de coroutine
                    _attackTimer =1000;
                    StartCoroutine(WaitEndAnim());
                    break;
                case ScyllaAttackState.attacking:
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
        _attackTimer = _attackTime;
    }
}

