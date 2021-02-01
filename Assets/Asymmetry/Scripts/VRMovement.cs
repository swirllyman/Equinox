using UnityEngine;
using Cinemachine;
using Asymmetry;

public class VRMovement : PlayerMovement
{
    Vector3 direction;
    void Update()
    {
        if (teleporting) return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (upsideDown)
        {
            if (isGrounded && velocity.y >= 0)
            {
                velocity.y = 2f;
            }
        }
        else
        {
            if (isGrounded && velocity.y <= 0)
            {
                velocity.y = -2f;
            }
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");


        if (upsideDown)
        {
            gravity = 9.81f;
            horizontal *= -1;
        }
        else
        {
            gravity = -9.81f;
        }

        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        freeLookCam.m_RecenterToTargetHeading.m_enabled = direction.magnitude < .1f;

        if (direction.magnitude >= .1f)
        {
            //direction
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + AsymmetricCrossPlatformController.singleton.vrListener.transform.eulerAngles.y;

            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
        }
        Vector2 secondaryAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        transform.Rotate(0, secondaryAxis.x * Time.deltaTime * spinSpeed, 0);
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
