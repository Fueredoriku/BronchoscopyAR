using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField]
    private Transform trackedObject;
    [SerializeField]
    private Transform orientationGizmo;
    [SerializeField]
    private Renderer gizmoMesh;
    private Vector3 gizmoScaleOriginal;
    [SerializeField]
    private GameObject[] lobeMesh;
    [SerializeField]
    private GameObject[] vesselMesh;
    [SerializeField]
    private GameObject[] PETMesh;
    [SerializeField]
    private GameObject[] landmarkMesh;
    [SerializeField]
    private GameObject[] extraMesh;
    private int mode = 0;
    [SerializeField]
    private GameObject[] faceUserUI;
    [SerializeField]
    private PinchSlider cutoutSlider;
    [SerializeField]
    private CutoutPath cutoutPath;

    //TODO:
    // - reset button above gizmo
    //      -> should reset position, rotation and scale
    // - less/more detail arrow on each side of gizmo
    private void Update()
    {
        transform.position = trackedObject.position;
        transform.localScale = trackedObject.localScale;
        orientationGizmo.rotation = trackedObject.rotation;
        foreach(var ui in faceUserUI)
            if (ui.activeInHierarchy)
                ui.transform.LookAt(Camera.main.transform.position);
    }
    private void Start()
    {
        gizmoScaleOriginal = gizmoMesh.transform.localScale;
    }
    public void EnableOrientationGizmo()
    {
        gizmoMesh.transform.localScale = Vector3.zero;
        LeanTween.scale(gizmoMesh.gameObject, gizmoScaleOriginal, 0.5f).setEaseInOutElastic();
        gizmoMesh.enabled = true;
        mode++;
        if (mode >= 4)
            mode = 0;
    }

    public void DisableOrientationGizmo()
    {
        gizmoMesh.enabled = false;
    }

    public void SetLobes()
    {
        foreach (var layer in lobeMesh)
            layer.SetActive(!layer.activeInHierarchy);
    }

    public void SetVessels()
    {
        foreach (var layer in vesselMesh)
            layer.SetActive(!layer.activeInHierarchy);
    }

    public void SetLandmarks()
    {
        foreach (var layer in landmarkMesh)
            layer.SetActive(!layer.activeInHierarchy);
    }

    public void SetPET()
    {
        foreach (var layer in PETMesh)
            layer.SetActive(!layer.activeInHierarchy);
    }

    public void SetExtra()
    {
        foreach (var layer in extraMesh)
            layer.SetActive(!layer.activeInHierarchy);
    }

    public void SetPathLength()
    {
        cutoutPath.NormalizedPathPosition = cutoutSlider.SliderValue;
    }

    public void SetCutOutdirection(int index)
    {
        cutoutPath.CutOutMode = (CutoutPath.CutOutDirection) Mathf.RoundToInt(Mathf.Clamp(index, 0f,4f));
    }

    public void ResetLayers()
    {
        foreach (var layer in lobeMesh)
            layer.SetActive(false);
        foreach (var layer in vesselMesh)
            layer.SetActive(false);
        foreach (var layer in landmarkMesh)
            layer.SetActive(false);
        foreach (var layer in PETMesh)
            layer.SetActive(false);
        foreach (var layer in extraMesh)
            layer.SetActive(false);
    }

    private void ToggleLayers(int mode)
    {
        switch (mode)
        {
            case 0:
                foreach (var layer in vesselMesh)
                    layer.SetActive(false);
                foreach (var layer2 in lobeMesh)
                    layer2.SetActive(false);
                break;
            case 1:
                foreach (var layer in vesselMesh)
                    layer.SetActive(true);
                foreach (var layer2 in lobeMesh)
                    layer2.SetActive(false);
                break;
            case 2:
                foreach (var layer in vesselMesh)
                    layer.SetActive(false);
                foreach (var layer2 in lobeMesh)
                    layer2.SetActive(true);
                break;
            case 3:
                foreach (var layer in vesselMesh)
                    layer.SetActive(true);
                foreach (var layer2 in lobeMesh)
                    layer2.SetActive(true);
                break;
        }

    }
}
