using UnityEngine;

public class BasicCrossPlatformInput : MonoBehaviour
{
    public static BasicCrossPlatformInput singleton;
    public delegate void HandGrab(int handID, bool down);
    public event HandGrab onHandInput;

    public string leftHandGrabString;
    public string rightHandGrabString;

    private void Awake()
    {
        singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown(leftHandGrabString))
        {
            onHandInput?.Invoke(0, true);
        }

        if (Input.GetButtonUp(leftHandGrabString))
        {
            onHandInput?.Invoke(0, false);
        }

        if (Input.GetButtonDown(rightHandGrabString))
        {
            onHandInput?.Invoke(1, true);
        }

        if (Input.GetButtonUp(rightHandGrabString))
        {
            onHandInput?.Invoke(1, false);
        }
    }
}
