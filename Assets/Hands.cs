using UnityEngine;

public class Hands : MonoBehaviour
{

    bool _hasRised = false;
    bool _hasHit = false;

    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.CompareTag("Player"))
        {
            if (!_hasRised)
            {
                _hasRised = true;
                GetComponent<Animator>().SetTrigger("Rise");
            }
            else
            {
                _hasHit = true;
                pOther.GetComponent<PlayerManager>().Hurt(1);
            }
        }
    }

    void OnTriggerExit(Collider pOther)
    {
        if (pOther.CompareTag("Player"))
        {
            if (!_hasHit) _hasRised = false;
            else _hasHit = false;
        }
    }
}
