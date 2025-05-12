using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerControls : MonoBehaviour
{
    [SerializeField] public float FrontSpeed;
    public bool ReverseXControls;
    [SerializeField] float _sideMoveLatency;
    [SerializeField] float _sailTurnSpeed;

    LanesManager _laneManager;
    bool _isMovingOnSide;
    bool _goingToNextLane;
    float _XTarget;
    Vector3 _SDVelocityRef;

    bool _sailTaken = false;
    Transform _sailTransform;
    WindManager _windManager;
    float _currentSailAngle;
    float _windAllignmentCoef = 0;


    void Start()
    {
        _laneManager = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        _sailTransform = transform.Find("Sail");
        _windManager = GameObject.FindGameObjectWithTag("WindManager").GetComponent<WindManager>();
        _currentSailAngle = _sailTransform.localEulerAngles.y % 360;
    }
    public void OnMoveInput(InputAction.CallbackContext pContext)
    {
        if (pContext.performed)
        {
            Vector2 vInputValue = pContext.ReadValue<Vector2>();
            if (ReverseXControls) vInputValue *= -1;

            float? vNewXTarget = _laneManager.GetNextLaneX((int)vInputValue.x);
            _goingToNextLane = vNewXTarget != null;
            _isMovingOnSide = true;
            _XTarget = _goingToNextLane ? (float)vNewXTarget : _XTarget + math.sign(vInputValue.x) * _laneManager.LaneWidth / 2;
        }
    }

    public void OnTakeSail(InputAction.CallbackContext pContext)
    {
        if (pContext.started) _sailTaken = true;
        if (pContext.canceled) _sailTaken = false;
    }

    public void OnTurnSail(InputAction.CallbackContext pContext)
    {
        //Si mouvement trop faible on ne fait rien
        if (!_sailTaken || math.abs(pContext.ReadValue<Vector2>().x) < 0.5f) return;

        //Calcul de la rotation correspondant au mouvement de souris (ou de pad) 
        Vector2 vMoveInput = pContext.ReadValue<Vector2>();
        float vRotation = (vMoveInput.x > 0 ? 1 : -1) * _sailTurnSpeed * Time.deltaTime;

        //Calcul du nouvel angle, remis en angle positif de 0 à 360 (pour faciliter les calculs)
        float vNewAngle = (_currentSailAngle + vRotation) % 360;
        if (vNewAngle < 0) vNewAngle += 360;

        //On controle qu'on reste bien dans les bornes de win (de -Max à +Max mais traduit en positif entre 0 et 360)
        if (vRotation > 0 && vNewAngle > _windManager.MaxWindAngle && _currentSailAngle < 180)
            vNewAngle = _windManager.MaxWindAngle;
        else if (vRotation < 0 && vNewAngle < (360 - _windManager.MaxWindAngle) && vNewAngle > 180)
            vNewAngle = 360 - _windManager.MaxWindAngle;

        //On applique la nouvelle rotation à la voile
        _sailTransform.rotation = Quaternion.Euler(new Vector3(_sailTransform.localEulerAngles.x, vNewAngle, _sailTransform.localEulerAngles.z));
        _currentSailAngle = vNewAngle;

        DebugTool.DrawDebugOnUI(2, "vCurrentAngle sail : " + _currentSailAngle.ToString("0.0"));
    }


    void Update()
    {
        //on avance tout droit
        transform.position += FrontSpeed * (_windAllignmentCoef + 1) * Time.deltaTime * Vector3.forward;

        //Eventuel deplacement côté
        if (_isMovingOnSide)
        {
            Vector3 vTempTargetPosition = new Vector3(_XTarget, transform.position.y, transform.position.z);

            if (_goingToNextLane)
                transform.position = Vector3.SmoothDamp(transform.position, vTempTargetPosition,
                    ref _SDVelocityRef, _sideMoveLatency);
            else
                transform.position = Vector3.MoveTowards(transform.position, vTempTargetPosition, _laneManager.LaneWidth / _sideMoveLatency * Time.deltaTime);

            if (Mathf.Abs(transform.position.x - _XTarget) < 0.05f)
            {
                transform.position = vTempTargetPosition;

                if (_goingToNextLane) _isMovingOnSide = false;
                else
                {
                    _XTarget = (float)_laneManager.GetLaneCenter(_laneManager.CurrentPlayerLane);
                    _goingToNextLane = true;
                }

            }
        }

        //Calcul du coefficient de prise de vent en fonction de l'angle du vent et de la voile

        //Les angles vont de 0 à 360, on décale leur valeur de façon à ce que l'angle 360-Max soit le 0 (pour pouvoir comparer des valeurs allant de 0 à la distance maximal, soit 2*Max)
        float vWindAngleFromZero = (_windManager.CurrentAngle + _windManager.MaxWindAngle) % 360;
        vWindAngleFromZero = math.clamp(vWindAngleFromZero, 0, 2 * _windManager.MaxWindAngle);
        float vSailAngleFromZero = (_currentSailAngle + _windManager.MaxWindAngle) % 360;
        vSailAngleFromZero = math.clamp(vSailAngleFromZero, 0, 2 * _windManager.MaxWindAngle);

        if (vWindAngleFromZero == 0 && vSailAngleFromZero == 0) _windAllignmentCoef = 1;
        else _windAllignmentCoef = 1 - math.abs(vSailAngleFromZero - vWindAngleFromZero) / (2 * _windManager.MaxWindAngle);

        DebugTool.DrawDebugOnUI(3, "wind coeff sail : " + _windAllignmentCoef.ToString("0.0"));
    }
}
