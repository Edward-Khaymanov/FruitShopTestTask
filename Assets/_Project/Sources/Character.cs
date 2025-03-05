using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private Transform _handObjectPosition;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _throwPower;

    private Rigidbody _rigidbody;
    private PickUpObject _objectInHand;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void MoveToDirection(Vector3 direction)
    {
        _rigidbody.velocity = direction * _moveSpeed;
    }

    public void RotateAdditive(Quaternion rotation)
    {
        _rigidbody.rotation *= rotation;
    }

    public void PickUpObject(PickUpObject pickUpObject)
    {
        if (_objectInHand != null)
            return;

        pickUpObject.Rigidbody.isKinematic = true;
        pickUpObject.Collider.enabled = false;
        pickUpObject.Transform.SetParent(_handObjectPosition);
        pickUpObject.Transform.localPosition = Vector3.zero;
        _objectInHand = pickUpObject;
    }

    public void ThrowItem()
    {
        if (_objectInHand == null)
            return;

        _objectInHand.Rigidbody.isKinematic = false;
        _objectInHand.Collider.enabled = true;
        _objectInHand.Transform.SetParent(null);
        _objectInHand.Rigidbody.AddForce(_handObjectPosition.transform.forward * _throwPower, ForceMode.Impulse);
        _objectInHand = null;
    }
}