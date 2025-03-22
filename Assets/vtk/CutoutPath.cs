using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CutoutPath : MonoBehaviour
{
    [SerializeField]
    private PathToTumorVisualizer path;
    private int index = 0;
    private Vector3[] travelPath;
    private void Start()
    {
        travelPath = path.SampledPath.ToArray();
        transform.SetLocalPositionAndRotation(path.positionOffset, Quaternion.Euler(path.rotationOffset));
    }
    void Update()
    {
        transform.GetChild(0).localPosition = -travelPath[index];
        index++;
        if (index == travelPath.Length-1)
            index = 0;
    }
}
