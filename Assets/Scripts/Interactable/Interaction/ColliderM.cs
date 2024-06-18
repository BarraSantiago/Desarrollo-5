using System;
using UnityEngine;

public class ColliderM : MonoBehaviour
{
    private Action<Collider> _triggerEnter = null;
    private Action<Collider> _triggerExit = null;

    public Action<Collider> TriggerEnter { get => _triggerEnter; set => _triggerEnter = value; }
    public Action<Collider> TriggerExit { get => _triggerExit; set => _triggerExit = value; }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name+ "enter to: OnTriggerEnter at: " +gameObject.name);
        _triggerEnter?.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        Debug.Log(other.gameObject.name + "exit to: OnTriggerExit at: " + gameObject.name);
        _triggerExit?.Invoke(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name + "enter to: OnCollisionEnter at: " + gameObject.name);
        _triggerEnter?.Invoke(collision.collider);
    }
    private void OnCollisionExit(Collision collision)
    {
        Debug.Log(collision.gameObject.name + "exit to: OnCollisionExit at: " + gameObject.name);
        _triggerExit?.Invoke(collision.collider);
    }
}
