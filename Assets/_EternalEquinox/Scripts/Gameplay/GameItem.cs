using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameItem : MonoBehaviour
{
    public int itemID = 0;
    public Rigidbody myBody;
    public Collider myCollider;
    public SubTrigger mySubTrigger;
    public bool lightItem;
    public AudioSource myAudioSource;

    Vector3 upVector;
    float currentGravity;
    bool thrown = false;

    private void Awake()
    {
        mySubTrigger.onTriggerEntered += SubTriggerEntered;
        mySubTrigger.onTriggerExited += SubTriggerExited;
        mySubTrigger.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (thrown)
        {
            mySubTrigger.transform.up = upVector;

            if(Vector3.Distance(transform.position, Vector3.zero) > 250f)
            {
                myBody.velocity = Vector3.zero;
                if (currentGravity > 0)
                {
                    transform.position = new Vector3(0, 1, 0);
                }
                else
                {
                    transform.position = new Vector3(0, -1.5f, 0);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        myAudioSource.Play();
    }

    public void FlipGravity()
    {
        currentGravity *= -1;
    }

    private void FixedUpdate()
    {
        if (thrown)
        {
            myBody.AddForce(Vector3.down * currentGravity);
        }
    }

    private void SubTriggerEntered(Collider collider)
    {
        if (collider == NetworkPlayer_THF.LocalPlayer.myCollider)
        {
            NetworkPlayer_THF.LocalPlayer.AddItemToNearbyList(this);
        }
    }

    private void SubTriggerExited(Collider collider)
    {
        if (collider == NetworkPlayer_THF.LocalPlayer.myCollider)
        {
            NetworkPlayer_THF.LocalPlayer.RemoveItemFromNearbyList(this);
        }
    }

    public void PlayerSpawn(int newItemID, bool sun = true)
    {
        itemID = newItemID;
        myBody.interpolation = RigidbodyInterpolation.None;
        myBody.isKinematic = true;
        mySubTrigger.gameObject.SetActive(false);
        thrown = false;
    }

    public void DropItem(Vector3 throwForce, Vector3 throwPosition, float gravity)
    {
        if(gravity > 0)
        {
            upVector = Vector3.up;
        }
        else
        {
            upVector = Vector3.down;
        }

        currentGravity = gravity;
        myBody.interpolation = RigidbodyInterpolation.Interpolate;
        myBody.isKinematic = false;
        transform.SetParent(null);
        transform.position = throwPosition;
        myBody.velocity = throwForce;
        mySubTrigger.gameObject.SetActive(true);
        thrown = true;
    }

    public void Pickup(Transform newParent)
    {
        transform.parent = newParent;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        myBody.interpolation = RigidbodyInterpolation.None;
        myBody.isKinematic = true;
        mySubTrigger.gameObject.SetActive(false);
        thrown = false;
    }

}
