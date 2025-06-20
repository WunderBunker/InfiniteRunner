using System.Collections.Generic;
using UnityEngine;

//ROCK
public class Rock : MonoBehaviour
{

    [SerializeField] List<AudioClip> _rockSounds = new();

    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.gameObject.CompareTag("Player"))
        {
            pOther.gameObject.GetComponent<PlayerManager>().Hurt(1);
            BiomesManager.Instance.RaiseNoise(1);
        }
        else if (pOther.gameObject.CompareTag("Bullet"))
        {
            pOther.gameObject.GetComponent<Projectile>().Explode();
            AudioManager.Instance.PlaySound(_rockSounds[new System.Random().Next(0, _rockSounds.Count)], 1);
        }
    }
}