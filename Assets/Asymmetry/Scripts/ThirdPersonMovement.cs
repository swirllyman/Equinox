using UnityEngine;

public class ThirdPersonMovement : PlayerMovement
{
    public Transform myHead;
    float turnSmoothVelocity;
    SimpleBodyIK myIK;

    private void Start()
    {
        myIK = GetComponentInChildren<SimpleBodyIK>();
    }
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

        float horizontal = Input.GetAxis("Horizontal_Keyboard");
        float vertical = Input.GetAxis("Vertical_Keyboard");

        if (upsideDown)
        {
            gravity = 9.81f;
            horizontal *= -1;
        }
        else
        {
            gravity = -9.81f;
        }

        freeLookCam.m_XAxis.Value = 0.0f;

        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        //freeLookCam.m_RecenterToTargetHeading.m_enabled = direction.magnitude < .1f;

        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;

        if(Cursor.lockState == CursorLockMode.Locked)
            transform.Rotate(0, Input.GetAxis("Mouse X") * spinSpeed, 0);

        if (direction.magnitude >= .15f)
        {
            Vector3 moveDir = Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z) * Vector3.forward;
            controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);

            myHead.transform.rotation = Quaternion.LookRotation(moveDir, upsideDown ? Vector3.down : Vector3.up);
        }

        //if (Input.GetButtonDown("Jump_Keyboard") && isGrounded)
        //{
        //    velocity.y = Mathf.Sqrt((jumpHeight * (upsideDown ? -1 : 1)) * (-2f * (upsideDown ? -1 : 1) * gravity);
        //}
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
