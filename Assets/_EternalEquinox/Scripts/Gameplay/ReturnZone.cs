using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class ReturnZone : MonoBehaviour
{
    public TMP_Text currentText;
    internal int itemsRemaining = 10;
    internal int zoneID = 0;

    internal bool finished = false;

    public delegate void SubTriggerEntered(int zoneID, ReturnZone zone, Collider collider);
    public event SubTriggerEntered onTriggerEntered;

    public delegate void SubTriggerExited(int zoneID, ReturnZone zone, Collider collider);
    public event SubTriggerExited onTriggerExited;

    public void Finished()
    {
        finished = true;
    }

    public void ResetZone() 
    {
        finished = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        onTriggerEntered?.Invoke(zoneID, this, other);
    }

    void OnTriggerExit(Collider other)
    {
        onTriggerExited?.Invoke(zoneID, this, other);
    }
}
