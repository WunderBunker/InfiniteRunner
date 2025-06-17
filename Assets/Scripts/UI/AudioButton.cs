using Unity.Mathematics;
using UnityEngine;

//BOUTONS D'INSTANCIATION DU MENU DES OPTIONS AUDIO 
public class AudioButton : MonoBehaviour
{
    [SerializeField] GameObject _soundMenu;
    public void OnButtonClick()
    {
        Instantiate(_soundMenu, transform.parent.position, quaternion.identity, transform.parent);
        AudioManager.Instance.PlayClickSound();
    }
}
