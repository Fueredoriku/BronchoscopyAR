using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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

    [SerializeField]
    private Transform anatomyParent;
    [SerializeField]
    private Transform anatomyHolder;
    [SerializeField]
    private Transform cutoutHolder;
    [SerializeField]
    private Transform cutoutTransform;
    private Quaternion relativeToParentRotation;
    private Quaternion relativeDirection;
    [SerializeField]
    private float testScale = 0.1f;
    private void Start()
    {
        travelPath = path.SampledPath.ToArray();
        transform.SetLocalPositionAndRotation(path.positionOffset, Quaternion.Euler(path.rotationOffset));
        oldPosition = travelPath[index];
        relativeToParentRotation = cutoutHolder.localRotation;
        relativeDirection = cutoutTransform.localRotation;
    }
    void Update()
    {
        index = Mathf.RoundToInt(Mathf.Lerp(1, travelPath.Length - 2, NormalizedPathPosition));
        Vector3 currentPosition = -travelPath[index];
        oldPosition = -travelPath[index - 1];
        Vector3 nextPosition = -travelPath[index + 1];
        //anatomyHolder.localPosition = cutoutHolder.rotation * -(currentPosition * testScale);
        switch (CutOutMode)
        {
            case CutOutDirection.objectUp:
                cutoutHolder.localPosition = currentPosition;
                cutoutHolder.rotation = anatomyParent.rotation;
                cutoutTransform.localRotation = relativeDirection;
                break;
            case CutOutDirection.objectForward:
                cutoutHolder.localPosition = currentPosition;
                cutoutHolder.localRotation = relativeToParentRotation;
                cutoutTransform.localRotation = relativeDirection;
                break;
            case CutOutDirection.camera:
                // TODO: All anatomy needs a holder parent which must have a postion offset akin to the pathpostion!!!
                // This must be done before rotating so the origin is adjuster for where on the path we are.
                //transform.parent.parent.localRotation *= Quaternion.FromToRotation(Vector3.up, Vector3.forward);
                //transform.parent.parent.localRotation *= Quaternion.FromToRotation(oldPosition - currentPosition + (currentPosition - nextPosition), Vector3.up);
                //cutoutHolder.LookAt(Camera.main.transform);
                //cutoutTransform.localRotation = Quaternion.FromToRotation(Vector3.up, Vector3.back);
                anatomyHolder.localPosition = Quaternion.FromToRotation(Vector3.up, Vector3.back) * Quaternion.FromToRotation(Vector3.forward, Vector3.back) * (-currentPosition * 0.25f);
                cutoutHolder.SetParent(anatomyParent);
                cutoutTransform.localRotation = Quaternion.FromToRotation(Vector3.down, oldPosition - currentPosition + (currentPosition - nextPosition));
                //cutoutHolder.localRotation = relativeToParentRotation;
                //anatomyHolder.localRotation = Quaternion.FromToRotation(Vector3.down, oldPosition - currentPosition + (currentPosition - nextPosition));

                //anatomyParent.LookAt(Camera.main.transform);
                /*
                anatomyParent.rotation = Quaternion.FromToRotation(Vector3.forward, anatomyParent.position - Camera.main.transform.position)
                    * Quaternion.Inverse(Quaternion.FromToRotation(Vector3.down, oldPosition - currentPosition + (currentPosition - nextPosition)));
                */
                break;
            case CutOutDirection.pathDirection:
                cutoutHolder.localPosition = currentPosition;
                cutoutHolder.localRotation = relativeToParentRotation;
                cutoutTransform.localRotation = Quaternion.FromToRotation(Vector3.down, oldPosition - currentPosition + (currentPosition - nextPosition)); 
                break;
            case CutOutDirection.pathDirectionTilted:
                cutoutHolder.localPosition = currentPosition;
                cutoutHolder.localRotation = Quaternion.FromToRotation(Vector3.up, Vector3.forward);
                cutoutTransform.localRotation = Quaternion.FromToRotation(Vector3.down, oldPosition - currentPosition + (currentPosition - nextPosition));
                break;
            default:
                break;
        }
    }
}
