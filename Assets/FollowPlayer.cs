using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField]float _followpLayerLatency;
    
    float _initialZSpacingWithPlayer;
    Transform _playertransform;
    Vector3 _SDVelocityRef;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _playertransform = GameObject.FindGameObjectWithTag("Player").transform;
        _initialZSpacingWithPlayer =  transform.position.z - _playertransform.position.z ;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 vTempTargetPosition = new Vector3(transform.position.x, transform.position.y, _playertransform.position.z +_initialZSpacingWithPlayer);
        
        transform.position = Vector3.SmoothDamp(transform.position, vTempTargetPosition,
            ref _SDVelocityRef, _followpLayerLatency);
    }
}
