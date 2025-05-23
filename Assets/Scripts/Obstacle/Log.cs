using System.Collections.Generic;
using UnityEngine;

public class Log : MonoBehaviour
{
    [SerializeField] List<AudioClip> _destroySounds = new();
    [SerializeField] GameObject _explosion;

    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.gameObject.CompareTag("Player"))
        {
            pOther.gameObject.GetComponent<PlayerManager>().Hurt(1);
            AudioManager.Instance.PlaySound(_destroySounds[new System.Random().Next(0, _destroySounds.Count)], 1);
            Instantiate(_explosion, transform.position, _explosion.transform.rotation, transform.parent);
            Destroy(gameObject);
        }
        else if (pOther.gameObject.CompareTag("Bullet"))
        {
            pOther.gameObject.GetComponent<Projectile>().Explode();
            AudioManager.Instance.PlaySound(_destroySounds[new System.Random().Next(0, _destroySounds.Count)], 1);
            Instantiate(_explosion, transform.position, _explosion.transform.rotation, transform.parent);
            Destroy(gameObject);
        }
    }
}
