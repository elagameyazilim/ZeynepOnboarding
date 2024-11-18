using System.Collections.Generic;
using UnityEngine;

public class StackSystem : MonoBehaviour
{
    public GameObject woodPrefab;
    public Transform stackPoint;
    public Transform buildPoint;
    private int woodCount = 0;
    private Queue<GameObject> pathQueue = new Queue<GameObject>();

    public int maxPathLength = 10;

    public void AddWood(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 newPosition = stackPoint.position + Vector3.up * woodCount * 0.1f;
            Instantiate(woodPrefab, newPosition, Quaternion.identity, stackPoint);
            woodCount++;
        }
    }

    public GameObject RemoveWood()
    {
        if (woodCount <= 0) return null;

        woodCount--;
        Transform lastWood = stackPoint.GetChild(woodCount);
        lastWood.SetParent(null);
        lastWood.gameObject.SetActive(true);
        return lastWood.gameObject;
    }

    public int GetWoodCount() => woodCount;

    public GameObject BuildPath()
    {
        if (woodCount <= 0) return null;

        GameObject wood = RemoveWood();
        if (wood != null)
        {
            wood.transform.position = buildPoint.position;
            wood.transform.rotation = Quaternion.identity;
            pathQueue.Enqueue(wood);

            buildPoint.position += Vector3.forward * (woodPrefab.transform.localScale.z - 0.01f);

            if (pathQueue.Count > maxPathLength)
            {
                Destroy(pathQueue.Dequeue());
            }
        }
        return wood;
    }
}