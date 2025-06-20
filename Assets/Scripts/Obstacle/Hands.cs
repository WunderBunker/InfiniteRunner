using System.Collections;
using UnityEngine;

//GESTION DE MAINS SPECTRALES
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
            //Première collision => détection du joueur dans la zone, on rise
            if (!_hasRised)
            {
                _hasRised = true;
                GetComponent<Animator>().SetTrigger("Rise");
                StartCoroutine(WaitRise());
            }
            //Déjà détecté => on fait du bruit
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

        //Si on n'a pas touché le joueur durant le rising alors on repasse en mode détection
        if (!_hasHit) _hasRised = false;
        else _hasHit = false;
    }

    void OnTriggerExit(Collider pOther)
    {
        if (pOther.CompareTag("Player"))
        {
            //Si le joueur sort de la zone de détetion sans avoir été touché on réactvie la possibilité de rise de noueveau
            if (!_hasHit) _hasRised = false;
            else _hasHit = false;
        }
    }

    void OnDestroy()
    {
        AudioManager.Instance.StopKeepSound(_bubbleSoundToken);
    }
}
