using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshColorEmitter : MonoBehaviour
{
    [SerializeField]
    private Transform spawnPoint;
    [SerializeField]
    private GameObject meshColoringPrefab;
    [SerializeField]
    private LayerMask colorLayer;
    private GameObject objectPool;
    private bool isSpawning = false;

    public void SpawnColoring()
    {
        if (objectPool) 
            Destroy(objectPool);
        isSpawning = true;
        objectPool = new("objectPool");
        StartCoroutine(Spawner());
    }

    private IEnumerator Spawner()
    {
        while (isSpawning) {
            if (Physics.OverlapSphere(spawnPoint.transform.position, 0.1f, colorLayer).Any())
            {
                var colorObject = Instantiate(meshColoringPrefab);
                colorObject.transform.position = spawnPoint.position;
                colorObject.transform.SetParent(objectPool.transform);
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void StopColoring()
    {
        isSpawning = false;
    }
}
