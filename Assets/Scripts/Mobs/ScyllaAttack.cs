using UnityEngine;

public class ScyllaAttack : MonoBehaviour
{
    [SerializeField] GameObject _preAttack;
    [SerializeField] float _preAttackTime;
    [SerializeField] GameObject _attack;
    [SerializeField] float _attackTime;
    float _attackTimer;

    Transform _playerTransform;
    float _attackRange;
    ScyllaAttackState _state;

    public float GetAttackRange()
    {
        return _attack.GetComponent<BoxCollider>().size.z * _attack.transform.localScale.z ;
    }

    void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _attackRange = GetAttackRange();

        _state = ScyllaAttackState.preparing;
        _attackTimer = _preAttackTime;
        //On divise l'attackRange par deux car le pivot est au milieu
        transform.position = new Vector3(transform.position.x, transform.position.y, _playerTransform.position.z - _attackRange/2 + 5);
        _preAttack.SetActive(true);
    }

    void Update()
    {
        _attackTimer -= Time.deltaTime;

        if (_attackTimer <= 0)
            switch (_state)
            {
                case ScyllaAttackState.preparing:
                    _attackTimer = _attackTime;
                    _state = ScyllaAttackState.attacking;

                    _preAttack.SetActive(false);
                    _attack.SetActive(true);
                    break;
                case ScyllaAttackState.attacking:
                    Destroy(gameObject);
                    return;
                default:
                    break;
            }
        
        //On divise l'attackRange par deux car le pivot est au milieu
        transform.position = new Vector3(transform.position.x, transform.position.y, _playerTransform.position.z - _attackRange/2+5);
    }
}

