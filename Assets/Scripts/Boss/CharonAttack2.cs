using System.Collections;
using UnityEngine;

//ATTAQUE SECONDAIRE DE CHARON (MAINS)
public class CharonAttack2 : MonoBehaviour
{
    [SerializeField] GameObject _preAttack;
    [SerializeField] float _preAttackTime;
    [SerializeField] GameObject _attack;
    [SerializeField] float _attackTime;
    [SerializeField] AudioClip _bubbleSound;
    float _attackTimer;
    bool _isAtSurface;

    Transform _playerTransform;
    AttackState _state;
    Animator _handsAnimator;
    private float _groundHeight;
    ParticleSystem _PS;
    int _bubbleSoundToken;

    public float GetAttackRange()
    {
        return _attack.GetComponent<BoxCollider>().size.z * _attack.transform.localScale.z;
    }

    void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _handsAnimator = transform.Find("Hands").GetComponent<Animator>();
        _groundHeight = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>().GroundHeight;

        _state = AttackState.preparing;
        _attackTimer = _preAttackTime;

        _PS = transform.Find("PreAttack").GetComponent<ParticleSystem>();

        transform.position = new Vector3(transform.position.x, transform.position.y, _playerTransform.position.z);
        _preAttack.SetActive(true);
        _PS.Play();

        _bubbleSoundToken = AudioManager.Instance.PlayKeepSound(_bubbleSound, 1, transform.position);
    }

    void Update()
    {
        //On remonte l'attaque si elle emmerge
        if (!_isAtSurface)
        {
            transform.position += new Vector3(0, Time.deltaTime * 10, 0);
            if (transform.position.y >= _groundHeight) _isAtSurface = true;
        }


        //Maj de l'état de l'attaque
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

    //On attend la fin de l'anim en cours avant de détruire l'objet
    IEnumerator WaitEndAnim()
    {
        yield return new WaitForSeconds(0.25f);
        _attack.SetActive(true);
        _attackTimer = _attackTime;
        yield return new WaitForSeconds(_handsAnimator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        AudioManager.Instance.StopKeepSound(_bubbleSoundToken);
    }
}