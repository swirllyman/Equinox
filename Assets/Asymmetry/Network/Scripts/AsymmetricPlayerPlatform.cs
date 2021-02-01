using UnityEngine;

/// <summary>
/// Asymmtric Class for handling Asymmetric game data
/// </summary>
public class AsymmetricPlayerPlatform : MonoBehaviour
{
    public static AsymmetricPlayerPlatform singleton;

    public AsymmetricPlatform platform_VR;
    public AsymmetricPlatform platform_PC;

    public MyHand[] vrHands;

    public void Awake()
    {
        singleton = this;

        //Force Mic Use to prompt permissions on android
        Microphone.Start("", false, 1, 44100);
    }

    public void TeleportPlayer(Vector3 teleportDestination)
    {

    }
}

[System.Serializable]
public struct AsymmetricPlatform
{
    public Transform head, leftHand, rightHand;
    public CharacterController characterController;
    public PlayerMovement playerMovement;
    public GameObject localPlayerVisuals;
}