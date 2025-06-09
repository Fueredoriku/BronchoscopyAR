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
        view,
        camera,
        pathDirection,
        pathDirectionTilted
    }
    [SerializeField]
    private PathToTumorVisualizer path;
    private int index = 0;
    private int oldIndex = 0;
    private Vector3[] travelPath;
    Vector3 oldPosition;
    [SerializeField, Range(0f, 1f)]
    public float NormalizedPathPosition = 0f;
    private CutOutDirection cutOutMode = CutOutDirection.pathDirection;
    public CutOutDirection CutOutMode 
    {
        get { return cutOutMode; }
        set 
        {
            cutoutAirway.SetActive(value == CutOutDirection.camera);
            path.SetPathAliveIndex(value == CutOutDirection.camera ? index : 1);
            cutoutHolder.localRotation = relativeToParentRotation;
            transform.SetParent(relativePivot);
            cutOutMode = value;
            relativePivot.localScale = Vector3.one;
            bronchocopeGizmo.localScale = bronchocopeGizmoScale;
        }
    }

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
    [SerializeField]
    private Transform bronchocopeGizmo;
    private Vector3 bronchocopeGizmoScale = new(0.025f, 0.025f, 0.025f);
    [SerializeField]
    private GameObject ctEstimate;
    private Material ctMaterial;
    private Quaternion relativeToParentRotation;
    private Quaternion relativeDirection;
    private void Start()
    {
        travelPath = path.SampledPath.ToArray();
        transform.SetLocalPositionAndRotation(path.positionOffset, Quaternion.Euler(path.rotationOffset));
        oldPosition = travelPath[index];
        relativeToParentRotation = cutoutHolder.localRotation;
        relativeDirection = cutoutTransform.localRotation;
        path.OnPathUpdated += UpdatePath;
        ctMaterial = Instantiate(ctEstimate.GetComponent<Renderer>().material);
        ctEstimate.GetComponent<Renderer>().material = ctMaterial;
        bronchocopeGizmoScale = bronchocopeGizmo.localScale;
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

        switch (CutOutMode)
        {
            case CutOutDirection.objectUp:
                cutoutHolder.localPosition = currentPosition;
                cutoutHolder.rotation = anatomyParent.rotation;
                cutoutTransform.localRotation = relativeDirection;
                break;
            case CutOutDirection.view:
                cutoutHolder.localPosition = currentPosition;
                cutoutHolder.LookAt(Camera.main.transform);
                cutoutTransform.localRotation = Quaternion.FromToRotation(Vector3.up, Vector3.back);
                break;
            case CutOutDirection.camera:
                
                cutoutHolder.localPosition = currentPosition;
                cutoutTransform.localRotation = Quaternion.FromToRotation(Vector3.down, oldPosition - nextPosition);

                Vector3 targetDir = (Camera.main.transform.position - cutoutTransform.position).normalized;
                Vector3 childForward = -cutoutTransform.up;

                Quaternion fromTo = Quaternion.FromToRotation(childForward, targetDir);
                Quaternion targetRotation = fromTo * relativePivot.rotation;

                relativePivot.rotation = Quaternion.Slerp(relativePivot.rotation, targetRotation, Time.deltaTime * 2f);
                var t = (float)index / travelPath.Length;
                var quint = t * t * t * t * t;
                relativePivot.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 4f, quint);
                bronchocopeGizmo.localScale = bronchocopeGizmoScale * (1f - quint) * 0.5f;
                if (oldIndex != index)
                    path.SetPathAliveIndex(index);
                oldIndex = index;
                break;
            case CutOutDirection.pathDirection:
                cutoutHolder.localPosition = currentPosition;
                cutoutHolder.localRotation = relativeToParentRotation;
                cutoutTransform.localRotation = Quaternion.FromToRotation(Vector3.down, oldPosition - nextPosition); 
                break;
            case CutOutDirection.pathDirectionTilted:
                cutoutHolder.localPosition = currentPosition;
                cutoutHolder.localRotation = Quaternion.FromToRotation(Vector3.up, Vector3.forward);
                cutoutTransform.localRotation = Quaternion.FromToRotation(Vector3.down, oldPosition - currentPosition + (currentPosition - nextPosition));
                break;
            default:
                break;
        }

        if (ctEstimate.activeInHierarchy)
        {
            Vector3 basePos = -travelPath[1];
            ctMaterial.SetVector("_RelativePosition", relativePivot.position);
            Quaternion inverseRotationQuat = Quaternion.Euler(180f, 0f, 90f) * Quaternion.Inverse(relativePivot.rotation); ;
            Vector4 inverseParentRotation = new (inverseRotationQuat.x, inverseRotationQuat.y, inverseRotationQuat.z, inverseRotationQuat.w);
            ctMaterial.SetVector("_RotationQuat", inverseParentRotation);
            ctMaterial.SetVector("_RelativeScale", new Vector4(1/relativePivot.localScale.x, 1/relativePivot.localScale.y, 1/relativePivot.localScale.z) * 2f);
        }
    }

    public void ResetCut()
    {
        cutOutMode = CutOutDirection.pathDirection;
        relativePivot.localRotation = Quaternion.identity;
        NormalizedPathPosition = 0f;
    }

    public void ToggleCT()
    {
        ctEstimate.SetActive(!ctEstimate.activeInHierarchy);
    }
}
