using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] float _followpLayerLatency = 0.25f;
    [SerializeField] float _switchDirectionTime = 1;

    float _initialZSpacingWithPlayer;
    Transform _playertransform;
    Vector3 _SDVelocityRef;

    bool _isBackward;
    bool _rotationIsFixed = true;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _playertransform = GameObject.FindGameObjectWithTag("Player").transform;
        _initialZSpacingWithPlayer = transform.position.z - _playertransform.position.z;
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 vTempTargetPosition = new Vector3(transform.position.x, transform.position.y, _playertransform.position.z);
        //Dans le cas du backward on augmente le spacing car smoothdamp a une inertie qui cause un retard sur l'axe z (car le player se déplace dans ce sens)
        vTempTargetPosition.z += _isBackward ? -_initialZSpacingWithPlayer*1.75f : _initialZSpacingWithPlayer; 

        if (!_rotationIsFixed)
        {
            vTempTargetPosition.x += math.PI * _initialZSpacingWithPlayer / _switchDirectionTime * Time.deltaTime;
            transform.Rotate(Vector3.up * 180 / _switchDirectionTime * Time.deltaTime, Space.World);

            if ((_isBackward && math.abs(transform.eulerAngles.y - 180) <= 2) || !_isBackward && math.abs(transform.eulerAngles.y) <= 5)
            {
                transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, _isBackward ? 180 : 0, transform.eulerAngles.z));
                _rotationIsFixed = true;
                _playertransform.GetComponent<playerControls>().IsBossMode = _isBackward;
            }
        }

        transform.position = Vector3.SmoothDamp(transform.position, vTempTargetPosition,
            ref _SDVelocityRef, _followpLayerLatency);
    }

    public void ChangeDirection()
    {
        _isBackward = !_isBackward;
        _rotationIsFixed = false;
    }

    public IEnumerator Tremour()
    {
        yield return null;
        float vAmplitude = 1;
        float vTime = 0.07f;

        for (int i = 0; i < 4; i++)
        {
            transform.position += new Vector3(vAmplitude, -vAmplitude, 0f);
            yield return new WaitForSeconds(vTime);
            transform.position += new Vector3(vAmplitude, vAmplitude, 0f);
            yield return new WaitForSeconds(vTime);
            transform.position += new Vector3(-vAmplitude, 0f, 0f);
            yield return new WaitForSeconds(vTime);
            transform.position += new Vector3(-vAmplitude, -vAmplitude, 0f);
            yield return new WaitForSeconds(vTime);
            transform.position += new Vector3(-vAmplitude, vAmplitude, 0f);
            yield return new WaitForSeconds(vTime);
            transform.position += new Vector3(vAmplitude, 0, 0f);
            yield return new WaitForSeconds(vTime);
            vAmplitude /= 2;
        }
    }
}
