using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CutoutPath : MonoBehaviour
{
    public enum CutOutDirection
    {
        objectUp,
        objectForward,
        camera,
        pathDirection,
        pathDirectionTilted
    }
    [SerializeField]
    private PathToTumorVisualizer path;
    private int index = 0;
    private Vector3[] travelPath;
    Vector3 oldPosition;
    [SerializeField, Range(0f, 1f)]
    public float NormalizedPathPosition = 0f;
    public CutOutDirection CutOutMode = CutOutDirection.objectUp;

    private Quaternion relativeToParentRotation;
    private Quaternion relativeDirection;
    private void Start()
    {
        travelPath = path.SampledPath.ToArray();
        transform.SetLocalPositionAndRotation(path.positionOffset, Quaternion.Euler(path.rotationOffset));
        oldPosition = travelPath[index];
        relativeToParentRotation = transform.GetChild(0).localRotation;
        relativeDirection = transform.GetChild(0).GetChild(0).localRotation;
    }
    void Update()
    {
        index = Mathf.RoundToInt(Mathf.Lerp(1, travelPath.Length - 2, NormalizedPathPosition));
        Vector3 currentPosition = -travelPath[index];
        oldPosition = -travelPath[index - 1];
        Vector3 nextPosition = -travelPath[index + 1];
        transform.GetChild(0).localPosition = currentPosition;

        switch (CutOutMode)
        {
            case CutOutDirection.objectUp:
                transform.GetChild(0).rotation = transform.parent.parent.rotation;
                transform.GetChild(0).GetChild(0).localRotation = relativeDirection;
                break;
            case CutOutDirection.objectForward:
                transform.GetChild(0).localRotation = relativeToParentRotation;
                transform.GetChild(0).GetChild(0).localRotation = relativeDirection;
                break;
            case CutOutDirection.camera:
                transform.GetChild(0).LookAt(Camera.main.transform);
                transform.GetChild(0).GetChild(0).localRotation = Quaternion.FromToRotation(Vector3.up, Vector3.back);
                break;
            case CutOutDirection.pathDirection:
                transform.GetChild(0).localRotation = relativeToParentRotation;
                transform.GetChild(0).GetChild(0).localRotation = Quaternion.FromToRotation(Vector3.down, oldPosition - currentPosition + (currentPosition - nextPosition)); 
                break;
            case CutOutDirection.pathDirectionTilted:
                transform.GetChild(0).localRotation = Quaternion.FromToRotation(Vector3.up, Vector3.forward);
                transform.GetChild(0).GetChild(0).localRotation = Quaternion.FromToRotation(Vector3.down, oldPosition - currentPosition + (currentPosition - nextPosition));
                break;
            default:
                break;
        }
    }
}
