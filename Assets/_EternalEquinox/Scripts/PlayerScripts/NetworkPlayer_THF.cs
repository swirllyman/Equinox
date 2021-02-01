using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer_THF : NetworkPlayer
{
    public static NetworkPlayer_THF LocalPlayer;
    public LayerMask standardLayer;
    public GameObject playerParticles;
    public Collider myCollider;
    public GameObject itemPrefab;
    public GameObject pickupUI;
    internal bool ableToSpawn = true;
    bool inPickupZone = false;
    public Transform[] itemHoldingOffsets;
    public AudioSource myAudio;
    public AudioClip[] playerAudioClips;

    bool[] readyToGrab = new bool[2];
    GameItem[] currentItems;

    List<GameItem> nearbyItems = new List<GameItem>();

    int itemSpawnCount = 10;

    public const int sampleFrameCount = 5;
    int currentFrame = 0;
    Vector3 velocityThisFrame;
    Vector3[] velocitySet;
    Vector3 prevPosFixed;
    public GameObject[] masks;

    protected override void Start()
    {
        base.Start();
        currentItems = new GameItem[2];
        pickupUI.SetActive(false);

        if (photonView.IsMine)
        {
            velocitySet = new Vector3[sampleFrameCount];
            BasicCrossPlatformInput.singleton.onHandInput += OnHandInput;
            LocalPlayer = this;
            photonView.RPC(nameof(SetMask_RPC), RpcTarget.AllBuffered, Random.Range(0, masks.Length));
        }
    }

    [PunRPC]
    void SetMask_RPC(int newMask)
    {
        if (photonView.IsMine && vrPlayer)
        {
            playerParticles.SetActive(false);
        }
        else
        {
            masks[newMask].SetActive(true);
            if (photonView.IsMine)
            {
                masks[newMask].gameObject.layer = standardLayer;
            }
        }
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            velocityThisFrame = (head.position - prevPosFixed) / Time.deltaTime;
            velocitySet[currentFrame] = velocityThisFrame;
            currentFrame = (currentFrame + 1) % velocitySet.Length;
            prevPosFixed = head.position;
        }
    }

    protected override void OnDestroy()
    {
        if (photonView.IsMine)
        {
            BasicCrossPlatformInput.singleton.onHandInput -= OnHandInput;
            LocalPlayer = null;
        }
        base.OnDestroy();
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

    private void OnHandInput(int handID, bool down)
    {
        if (!down)
        {
            readyToGrab[handID] = false;
        }
        if(down &!inPickupZone && ableToSpawn && currentItems[handID] == null && itemSpawnCount > 0)
        {
            itemSpawnCount--;
            photonView.RPC(nameof(SpawnNewItem_RPC), RpcTarget.AllViaServer, handID);
        }
        else if(!down && currentItems[handID] != null)
        {
            readyToGrab[handID] = false;
            Vector3 vel, pos;
            if (vrPlayer)
            {
                MyHand myHand = AsymmetricPlayerPlatform.singleton.vrHands[handID];
                vel = (myHand.GetWorldVelocity() - GetWorldVelocity()) * 2.5f;
                pos = myHand.transform.position;
            }
            else
            {
                vel = head.transform.forward.normalized * 2.5f;
                pos = itemHoldingOffsets[handID].position;
            }
            photonView.RPC(nameof(ThrowItem_RPC), RpcTarget.All, handID, vel, pos, myPlayerMovement.GetCurrentGravity());
        }
        else if(down && inPickupZone && nearbyItems.Count > 0)
        {
            PickupItem(handID, nearbyItems[0]);
        }
        else if (down && currentItems[handID] == null)
        {
            readyToGrab[handID] = true;
        }
    }

    void PickupItem(int handID, GameItem item)
    {
        readyToGrab[handID] = false;
        RemoveItemFromNearbyList(item);
        photonView.RPC(nameof(PickupItem_RPC), RpcTarget.All, handID, item.itemID);
    }

    public void AddItemToNearbyList(GameItem item)
    {
        inPickupZone = true;
        nearbyItems.Add(item);
        pickupUI.SetActive(true);

        for (int i = 0; i < readyToGrab.Length; i++)
        {
            print(i + " grab " + readyToGrab[i]);
            if (readyToGrab[i])
            {
                if(nearbyItems[0] != null)
                    PickupItem(i, nearbyItems[0]);
                break;
            }
        }
    }

    public void RemoveItemFromNearbyList(GameItem item)
    {
        if(nearbyItems.Contains(item))
            nearbyItems.Remove(item);

        if (nearbyItems.Count <= 0)
        {
            inPickupZone = false;
            pickupUI.SetActive(false);
        }
    }

    [PunRPC]
    void PickupItem_RPC(int handID, int itemID)
    {
        GameItem item = GameManager.singleton.gameItemList[itemID];
        item.Pickup(itemHoldingOffsets[handID]);
        currentItems[handID] = item;

        myAudio.PlayOneShot(playerAudioClips[0]);
    }

    [PunRPC]
    void SpawnNewItem_RPC(int handID)
    {
        GameObject newItem = Instantiate(itemPrefab, itemHoldingOffsets[handID]);
        GameItem item = newItem.GetComponent<GameItem>();
        item.PlayerSpawn(GameManager.singleton.GetNewItemID(item));
        currentItems[handID] = item;
        myAudio.PlayOneShot(playerAudioClips[0]);
    }

    [PunRPC]
    void ThrowItem_RPC(int handID, Vector3 velocity, Vector3 throwPosition, float gravity)
    {
        if (currentItems == null) return;
        currentItems[handID].DropItem(velocity, throwPosition, gravity);
        currentItems[handID] = null;
        myAudio.PlayOneShot(playerAudioClips[1]);
    }
}
