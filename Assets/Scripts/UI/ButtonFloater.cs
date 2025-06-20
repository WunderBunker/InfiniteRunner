using Unity.Mathematics;
using UnityEngine;

//GESTION DE LA FLOATTAISON DES BOUTONS UI
public class ButtonFloater : MonoBehaviour
{
    Transform _parent;
    [SerializeField] float _speed = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _parent = transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        _parent.position += new Vector3(0, Time.unscaledDeltaTime * math.sin(Time.unscaledTime * 3) * _speed, 0);
    }
}
