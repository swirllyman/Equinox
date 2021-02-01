using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puddle : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GameItem"))
        {
            other.GetComponent<GameItem>().FlipGravity();
        }
    }
}
