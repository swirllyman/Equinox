using OVRTouchSample;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyHand : MonoBehaviour
{
    public int handID;

    bool flexing, pinching, pointing, thumbsUpping;

    public delegate void UpdateHandPose(int handID, float flex, float pinch, float point, float thumbsUp);
    public event UpdateHandPose onUpdateHandPose;

    public const string ANIM_LAYER_NAME_POINT = "Point Layer";
    public const string ANIM_LAYER_NAME_THUMB = "Thumb Layer";
    public const string ANIM_PARAM_NAME_FLEX = "Flex";
    public const string ANIM_PARAM_NAME_POSE = "Pose";
    public const float INPUT_RATE_CHANGE = 20.0f;

    [SerializeField]
    private OVRInput.Controller controllerType = OVRInput.Controller.None;
    [SerializeField]
    private Animator anim = null;
    List<Renderer> showAfterInputFocusAcquired;

    private int animLayerIndexThumb = -1;
    private int animLayerIndexPoint = -1;
    private int animParamIndexFlex = -1;

    private bool isPointing = false;
    private bool isGivingThumbsUp = false;
    private float pointBlend = 0.0f;
    private float thumbsUpBlend = 0.0f;

    private bool restoreOnInputAcquired = false;

    public const int sampleFrameCount = 5;
    int currentFrame = 0;
    Vector3 velocityThisFrame;
    Vector3[] velocitySet;
    Vector3 prevPosFixed;

    private void Awake()
    {
        //Keep Function Here for Override purposes
    }

    private void Start()
    {
        showAfterInputFocusAcquired = new List<Renderer>();
        velocitySet = new Vector3[sampleFrameCount];

        // Get animator layer indices by name, for later use switching between hand visuals
        animLayerIndexPoint = anim.GetLayerIndex(ANIM_LAYER_NAME_POINT);
        animLayerIndexThumb = anim.GetLayerIndex(ANIM_LAYER_NAME_THUMB);
        animParamIndexFlex = Animator.StringToHash(ANIM_PARAM_NAME_FLEX);

        OVRManager.InputFocusAcquired += OnInputFocusAcquired;
        OVRManager.InputFocusLost += OnInputFocusLost;
#if UNITY_EDITOR
        OVRPlugin.SendEvent("custom_hand", (SceneManager.GetActiveScene().name == "CustomHands").ToString(), "sample_framework");
#endif
    }

    private void OnDestroy()
    {
        OVRManager.InputFocusAcquired -= OnInputFocusAcquired;
        OVRManager.InputFocusLost -= OnInputFocusLost;
    }

    private void Update()
    {
        UpdateCapTouchStates();

        pointBlend = InputValueRateChange(isPointing, pointBlend);
        thumbsUpBlend = InputValueRateChange(isGivingThumbsUp, thumbsUpBlend);
        UpdateAnimStates();
    }

    private void FixedUpdate()
    {
        velocityThisFrame = (transform.position - prevPosFixed) / Time.deltaTime;
        velocitySet[currentFrame] = velocityThisFrame;
        currentFrame = (currentFrame + 1) % velocitySet.Length;
        prevPosFixed = transform.position;
    }

    // Just checking the state of the index and thumb cap touch sensors, but with a little bit of
    // debouncing.
    private void UpdateCapTouchStates()
    {
        isPointing = !OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, controllerType);
        isGivingThumbsUp = !OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons, controllerType);
    }

    private void LateUpdate()
    {
        //Keep Function Here for Override purposes
    }

    // Simple Dash support. Just hide the hands.
    private void OnInputFocusLost()
    {
        if (gameObject.activeInHierarchy)
        {
            showAfterInputFocusAcquired.Clear();
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; ++i)
            {
                if (renderers[i].enabled)
                {
                    renderers[i].enabled = false;
                    showAfterInputFocusAcquired.Add(renderers[i]);
                }
            }

            restoreOnInputAcquired = true;
        }
    }

    private void OnInputFocusAcquired()
    {
        if (restoreOnInputAcquired)
        {
            for (int i = 0; i < showAfterInputFocusAcquired.Count; ++i)
            {
                if (showAfterInputFocusAcquired[i])
                {
                    showAfterInputFocusAcquired[i].enabled = true;
                }
            }
            showAfterInputFocusAcquired.Clear();

            restoreOnInputAcquired = false;
        }
    }

    private float InputValueRateChange(bool isDown, float value)
    {
        float rateDelta = Time.deltaTime * INPUT_RATE_CHANGE;
        float sign = isDown ? 1.0f : -1.0f;
        return Mathf.Clamp01(value + rateDelta * sign);
    }

    void UpdateAnimStates()
    {
        bool broadcastMajorUpdate = false;

        // Flex
        float flex = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controllerType);
        anim.SetFloat(animParamIndexFlex, flex);

        // Point
        float point = pointBlend;
        //print("Point " +point);
        anim.SetLayerWeight(animLayerIndexPoint, point);

        // Thumbs up
        float thumbsUp = thumbsUpBlend;
        anim.SetLayerWeight(animLayerIndexThumb, thumbsUp);

        float pinch = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controllerType);
        anim.SetFloat("Pinch", pinch);


        //Check For Major Changes
        if (thumbsUp >= .95f & !thumbsUpping)
        {
            thumbsUpping = true;
            broadcastMajorUpdate = true;
        }
        else if (thumbsUp < .05f && thumbsUpping)
        {
            thumbsUpping = false;
            broadcastMajorUpdate = true;
        }

        if (point >= .95f & !pointing)
        {
            pointing = true;
            broadcastMajorUpdate = true;
        }
        else if (point < .05f && pointing)
        {
            pointing = false;
            broadcastMajorUpdate = true;
        }

        if (flex >= .95f & !flexing)
        {
            flexing = true;
            broadcastMajorUpdate = true;
        }
        else if (flex < .05f && flexing)
        {
            flexing = false;
            broadcastMajorUpdate = true;
        }

        if (pinch >= .95f & !pinching)
        {
            broadcastMajorUpdate = true;
            pinching = true;
        }
        else if (pinch < .05f && pinching)
        {
            pinching = false;
            broadcastMajorUpdate = true;
        }

        if (broadcastMajorUpdate)
        {
            onUpdateHandPose?.Invoke(handID, flex, pinch, point, thumbsUp);
        }
    }

    public Vector3 GetWorldVelocity()
    {
        Vector3 avgVelocity = Vector3.zero;
        for (int i = 0; i < velocitySet.Length; i++)
        {
            avgVelocity += velocitySet[i];
        }

        avgVelocity /= velocitySet.Length;
        return avgVelocity;
    }
}
