using Asymmetry;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Material daytimeSkybox;
    public Material nightSkybox;
    public Transform cameraTransform;
    public CinemachineFreeLook freeLookCam;

    public Transform groundCheck;
    public LayerMask groundMask;
    public float gravity = -9.81f;
    public float groundDistance = .4f;
    public float moveSpeed = 6.0f;
    public float turnSmoothTime = .1f;
    public float jumpHeight = 3.0f;
    public float spinSpeed = 360.0f;

    protected bool teleporting = false;
    protected bool isGrounded = false;
    protected bool upsideDown = false;
    protected Vector3 velocity;

    protected CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void TeleportPlayer(Transform destination)
    {
        controller.enabled = false;
        transform.position = destination.position;
        transform.rotation = destination.rotation;
        controller.enabled = true;

        StartCoroutine(AllowMovementAfterTeleport());
        upsideDown = !upsideDown;
        RenderSettings.skybox = upsideDown ? nightSkybox : daytimeSkybox;
    }

    IEnumerator AllowMovementAfterTeleport()
    {
        yield return new WaitForSeconds(1.0f);
        teleporting = false;
    }

    public float GetCurrentGravity()
    {
        return upsideDown ? -9.81f : 9.81f;
    }
}
