using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    public class TouchData
    {
        public int FingerId;
        public float BeganTime;
        public bool BeganOverUI;
    }

    [SerializeField] private Character _character;
    [SerializeField] private Camera _characterCamera;
    [SerializeField] private Joystick _moveJoystick;
    [SerializeField] private Button _dropItemButton;
    [SerializeField] private float _rotateSensitivity = 4f;
    [SerializeField] private float _rotateMinYAngle = -60f;
    [SerializeField] private float _rotateMaxYAngle = 60f;

    private LayerMask _interactableObjectsMask;
    private int _rotationFingerId = -1;
    private readonly List<TouchData> _touchesData = new();

    private void Awake()
    {
        _interactableObjectsMask = LayerMask.GetMask(CONSTANTS.INTERACTABLE_OBJECTS_LAYER_NAME);
    }

    private void OnEnable()
    {
        _dropItemButton.onClick.AddListener(OnThrowItemClicked);
    }

    private void OnDisable()
    {
        _dropItemButton.onClick.RemoveListener(OnThrowItemClicked);
    }

    private void Update()
    {
        AddNewTouches();
        UpdateRotationFinger();
        TryInteract();
        Move();
        TryRotate();
        RemoveEndedTouches();
    }

    private void AddNewTouches()
    {
        foreach (var touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                _touchesData.Add(new TouchData()
                {
                    FingerId = touch.fingerId,
                    BeganTime = Time.unscaledTime,
                    BeganOverUI = EventSystem.current.IsPointerOverGameObject(touch.fingerId)
                });
            }
        }
    }

    private void RemoveEndedTouches()
    {
        foreach (var touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Ended)
            {
                var touchData = _touchesData.FirstOrDefault(x => x.FingerId == touch.fingerId);
                _touchesData.Remove(touchData);
            }
        }
    }

    private void UpdateRotationFinger()
    {
        if (_rotationFingerId != -1)
        {
            var rotationTouch = Utilities.GetTouchByFingerId(_rotationFingerId);
            if (rotationTouch.HasValue)
            {
                if (Utilities.EnumInRange(rotationTouch.Value.phase, TouchPhase.Ended, TouchPhase.Canceled))
                {
                    _rotationFingerId = -1;
                }
                else
                {
                    return;
                }
            }
        }

        foreach (var touch in Input.touches)
        {
            if (Utilities.EnumInRange(touch.phase, TouchPhase.Moved, TouchPhase.Stationary) == false)
                continue;

            var touchData = _touchesData.FirstOrDefault(x => x.FingerId == touch.fingerId);
            if (touchData.BeganOverUI)
                continue;

            _rotationFingerId = touch.fingerId;
            break;
        }
    }

    private void TryRotate()
    {
        if (Input.touchCount <= 0)
            return;

        if (_rotationFingerId == -1)
            return;

        var targetTouch = Utilities.GetTouchByFingerId(_rotationFingerId).Value;
        var normalizedDelta = targetTouch.deltaPosition / Screen.dpi;
        normalizedDelta /= Time.deltaTime;
        normalizedDelta = Vector2.ClampMagnitude(normalizedDelta, 1f);
        normalizedDelta *= _rotateSensitivity;

        var yRotation = Quaternion.Euler(0f, normalizedDelta.x * _rotateSensitivity, 0f);
        _character.RotateAdditive(yRotation);

        var currentRotationX = _characterCamera.transform.localEulerAngles.x - normalizedDelta.y * _rotateSensitivity;
        if (currentRotationX > 180)
            currentRotationX -= 360;

        currentRotationX = Mathf.Clamp(currentRotationX, _rotateMinYAngle, _rotateMaxYAngle);
        _characterCamera.transform.localRotation = Quaternion.Euler(currentRotationX, 0f, 0f);
    }

    private void Move()
    {
        var direction = _characterCamera.transform.forward * _moveJoystick.Direction.y + _characterCamera.transform.right * _moveJoystick.Direction.x;
        _character.MoveToDirection(direction.normalized);
    }

    private void TryInteract()
    {
        if (Input.touchCount <= 0)
            return;

        foreach (var touch in Input.touches)
        {
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;

            if (touch.phase != TouchPhase.Ended)
                return;

            var touchState = _touchesData.FirstOrDefault(x => x.FingerId == touch.fingerId);
            if (touchState.BeganOverUI)
                return;

            if (Time.unscaledTime - touchState.BeganTime > CONSTANTS.DEFAULT_TAP_TIME)
                return;

            var ray = Camera.main.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, _interactableObjectsMask))
            {
                if (hit.collider.TryGetComponent<InteractableObject>(out var component))
                {
                    if (component is PickUpObject pickUpObject)
                    {
                        _character.PickUpObject(pickUpObject);
                        _dropItemButton.gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    private void OnThrowItemClicked()
    {
        _character.ThrowItem();
        _dropItemButton.gameObject.SetActive(false);
    }
}