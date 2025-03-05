using UnityEngine;

public class PickUpObject : InteractableObject
{
    public GameObject GameObject { get; private set; }
    public Transform Transform { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public Collider Collider { get; private set; }

    private void Awake()
    {
        GameObject = this.gameObject;
        Transform = transform;
        Rigidbody = GetComponent<Rigidbody>();
        Collider = GetComponent<Collider>();
    }
}