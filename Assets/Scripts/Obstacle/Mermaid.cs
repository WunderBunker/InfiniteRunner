using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mermaid : MonoBehaviour
{
    [SerializeField] float _chantTime;
    [SerializeField] float _chantTempo;
    [SerializeField] float _chantRange;
    [SerializeField] List<AudioClip> _hurtSounds = new();
    [SerializeField] List<AudioClip> _chantSounds = new();

    LanesManager _LM;
    GameObject _player;

    bool _isChanting;
    float _timer;

    byte _currentLane;

    ParticleSystem _PS;

    GameObject _body;
    bool _isDead;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");

        _LM = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        _currentLane = _LM.GetLaneFromXPos(transform.position.x);

        _body = transform.Find("Body").gameObject;

        _PS = transform.Find("Particles").GetComponent<ParticleSystem>();
        ParticleSystem.ShapeModule vShape = _PS.shape;
        vShape.radius = _chantRange;
        transform.Find("Particles").localPosition = new Vector3(0, 0, -_chantRange + 1);

        _timer = _chantTime;
        _isChanting = true;
        _PS.Play();
        AudioManager.Instance.PlaySound(_chantSounds[new System.Random().Next(0, _chantSounds.Count)], 1, transform.position);
    }

    // Update is called once per frame
    void Update()
    {

        if (_isDead) return;

        _timer -= Time.deltaTime;
        if (_isChanting)
        {

            if (transform.position.z - _player.transform.position.z >= 0 && transform.position.z - _player.transform.position.z <= 2*_chantRange)
                _player.GetComponent<PlayerControls>().BlockLane(_currentLane, _timer);

            if (_timer <= 0)
            {
                _timer = _chantTempo;
                _isChanting = false;

                _PS.Stop();
            }
        }
        else if (_timer <= 0)
        {
            _timer = _chantTime;
            _isChanting = true;
            _PS.Play();
            AudioManager.Instance.PlaySound(_chantSounds[new System.Random().Next(0, _chantSounds.Count)], 1, transform.position);
        }
    }

    void OnTriggerEnter(Collider pOther)
    {
        if (_isDead) return;

        if (pOther.CompareTag("Player")) pOther.GetComponent<PlayerManager>().Hurt(1);
        else if (pOther.CompareTag("Bullet"))
        {
            pOther.gameObject.GetComponent<Projectile>().Explode();
            StartCoroutine(Death());
        }
    }

    IEnumerator Death()
    {
        _isDead = true;
        AudioManager.Instance.PlaySound(_hurtSounds[new System.Random().Next(0, _hurtSounds.Count)], 1, transform.position);
        _body.GetComponent<Animator>().SetTrigger("Death");
        _player.GetComponent<PlayerControls>().BlockLane(_currentLane, 0);
        yield return new WaitForSeconds(0.1f);
        yield return new WaitForSeconds(_body.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }
}
