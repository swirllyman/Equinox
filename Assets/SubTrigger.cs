using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubTrigger : MonoBehaviour
{
    public delegate void SubTriggerEntered(Collider collider);
    public event SubTriggerEntered onTriggerEntered;

    public delegate void SubTriggerExited(Collider collider);
    public event SubTriggerExited onTriggerExited;

    private void OnTriggerEnter(Collider other)
    {
        onTriggerEntered?.Invoke(other);
    }

    void OnTriggerExit(Collider other)
    {
        onTriggerExited?.Invoke(other);
    }
}
