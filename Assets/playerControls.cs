using UnityEngine;
using UnityEngine.InputSystem;

public class playerControls : MonoBehaviour
{
    [SerializeField] float _frontSpeed;
    [SerializeField] float _sideMoveLatency;

    LanesManager _laneManager;
    bool _isMovingOnSide;
    float _XTarget;
    Vector3 _SDVelocityRef;

    void Start()
    {
        _laneManager = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
    }
    public void OnMoveInput(InputAction.CallbackContext pContext)
    {
        if (pContext.performed)
        {
            Vector2 vInputValue = pContext.ReadValue<Vector2>();

            float? vNewXTarget = _laneManager.GetNextLaneX((int)vInputValue.x);
            if (vNewXTarget != null)
            {
                _isMovingOnSide = true;
                _XTarget = (float)vNewXTarget;
            }
        }
    }

    void Update()
    {
        //on avance tout droit
        transform.position += _frontSpeed * Time.deltaTime * Vector3.forward;

        if (_isMovingOnSide)
        {
            Vector3 vTempTargetPosition = new Vector3(_XTarget, transform.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, vTempTargetPosition,
                ref _SDVelocityRef, _sideMoveLatency);

            if(Mathf.Abs(transform.position.x-_XTarget)<0.05f)
            {
                transform.position = vTempTargetPosition;
                _isMovingOnSide = false;
            }
        }
    }
}
