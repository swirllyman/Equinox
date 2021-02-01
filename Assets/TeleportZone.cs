using Asymmetry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportZone : MonoBehaviour
{
    public Transform destination;
    public AudioSource teleportZoneAudio;
    public int destinationZone = 0;

    private void OnTriggerEnter(Collider other)
    {
        //print("Entered: " + other.name);
        if (other == AsymmetricPlayerPlatform.singleton.platform_PC.characterController) 
        {
            AsymmetricPlayerPlatform.singleton.platform_PC.playerMovement.TeleportPlayer(destination);
            //AsymmetricCrossPlatformController.singleton.cinemachineBrian.m_WorldUpOverride = destination;
            teleportZoneAudio.Play();
            GameManager.singleton.SetZone(destinationZone);
        }
        else if(other == AsymmetricPlayerPlatform.singleton.platform_VR.characterController)
        {
            AsymmetricPlayerPlatform.singleton.platform_VR.playerMovement.TeleportPlayer(destination);
            //if(AsymmetricCrossPlatformController.singleton.playerType == 0)
            //    AsymmetricCrossPlatformController.singleton.cinemachineBrian.m_WorldUpOverride = destination;
            teleportZoneAudio.Play();
            GameManager.singleton.SetZone(destinationZone);
        }
    }
}
