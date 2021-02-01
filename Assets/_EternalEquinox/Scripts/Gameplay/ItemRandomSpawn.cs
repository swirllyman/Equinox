using Photon.Pun;
using UnityEngine;

public class ItemRandomSpawn : MonoBehaviourPun
{
    public GameObject itemPrefab;
    public int maxSpawns = 10;
    public bool upsideDown = false;

    [ContextMenu("SpawnItems")]
    public void SpawnItems()
    {
        if (photonView.IsMine)
        {
            Vector3[] spawnArray = GetSpawnArray();
            photonView.RPC(nameof(SpawnItems_RPC), RpcTarget.All, spawnArray);
        }
    }

    [PunRPC]
    void SpawnItems_RPC(Vector3[] spawnPositions)
    {
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            GameObject newItem = Instantiate(itemPrefab, spawnPositions[i], Quaternion.identity);
            GameItem item = newItem.GetComponent<GameItem>();
            item.itemID = GameManager.singleton.GetNewItemID(item);
            item.lightItem = !upsideDown;
            item.DropItem(Vector3.zero, spawnPositions[i], upsideDown ? -9.81f : 9.81f);
        }
    }


    Vector3[] GetSpawnArray()
    {
        Vector3[] spawnArray = new Vector3[maxSpawns];
        for (int i = 0; i < spawnArray.Length; i++)
        {
            spawnArray[i] = new Vector3(Random.Range(-7.0f, 7.0f), transform.position.y, Random.Range(-7.0f, 7.0f));
        }
        return spawnArray;
    }
}
