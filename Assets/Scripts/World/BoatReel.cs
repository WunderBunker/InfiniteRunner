using Unity.Mathematics;
using UnityEngine;

public class BoatReel : MonoBehaviour
{
    [SerializeField] float _speed=1;

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(0,_speed*Time.deltaTime * math.sin(Time.time*2), 0);
    }
}
