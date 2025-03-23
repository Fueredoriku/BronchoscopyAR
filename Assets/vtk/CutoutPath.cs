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
    Vector3 oldPositon;
    private void Start()
    {
        travelPath = path.SampledPath.ToArray();
        transform.SetLocalPositionAndRotation(path.positionOffset, Quaternion.Euler(path.rotationOffset));
        oldPositon = travelPath[index];
    }
    void Update()
    {
        Vector3 currentPosition = -travelPath[index];
        transform.GetChild(0).localPosition = currentPosition;
        transform.GetChild(0).up = Vector3.Cross(transform.right, oldPositon - currentPosition);
        index++;
        if (index == travelPath.Length-1)
            index = 0;
        oldPositon = currentPosition;
    }
}
