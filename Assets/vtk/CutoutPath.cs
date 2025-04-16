using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
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
    private Transform relativePivot;
    [SerializeField]
    private Transform cutoutHolder;
    [SerializeField]
    private GameObject cutoutAirway;
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
        path.OnPathUpdated += UpdatePath;
    }

    private void UpdatePath()
    {
        travelPath = path.SampledPath.ToArray();
    }

    private void OnDestroy()
    {
        path.OnPathUpdated -= UpdatePath;
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
                cutoutAirway.SetActive(true);
                transform.SetParent(relativePivot);
                cutoutHolder.localPosition = currentPosition;
                cutoutTransform.localRotation = Quaternion.FromToRotation(Vector3.down, oldPosition - currentPosition + (currentPosition - nextPosition));

                Vector3 targetDir = (Camera.main.transform.position - cutoutTransform.position).normalized;
                Vector3 childForward = -cutoutTransform.up;

                Quaternion fromTo = Quaternion.FromToRotation(childForward, targetDir);
                Quaternion targetRotation = fromTo * relativePivot.rotation;

                relativePivot.rotation = Quaternion.Slerp(relativePivot.rotation, targetRotation, Time.deltaTime * 2f);
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
