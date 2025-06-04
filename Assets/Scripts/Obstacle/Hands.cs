using System.Collections;
using UnityEngine;

public class Hands : MonoBehaviour
{
    [SerializeField] AudioClip _bubbleSound;
    [SerializeField] AudioClip _screamSound;
    bool _hasRised = false;
    bool _hasHit = false;
    int _bubbleSoundToken;


    void Start()
    {
        _bubbleSoundToken = AudioManager.Instance.PlayKeepSound(_bubbleSound, 1, transform.position);
    }

    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.CompareTag("Player"))
        {
            if (!_hasRised)
            {
                _hasRised = true;
                GetComponent<Animator>().SetTrigger("Rise");
                StartCoroutine(WaitRise());
            }
            else
            {
                _hasHit = true;
                BiomesManager.Instance.RaiseNoise(3);
                AudioManager.Instance.PlaySound(_screamSound, 1);
            }
        }
    }

    IEnumerator WaitRise()
    {
        yield return new WaitForSeconds(0.1f);
        yield return new WaitForSeconds(GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);

        if (!_hasHit) _hasRised = false;
        else _hasHit = false;
    }

    void OnTriggerExit(Collider pOther)
    {
        if (pOther.CompareTag("Player"))
        {
            if (!_hasHit) _hasRised = false;
            else _hasHit = false;
        }
    }

    void OnDestroy()
    {
        AudioManager.Instance.StopKeepSound(_bubbleSoundToken);
    }
}
