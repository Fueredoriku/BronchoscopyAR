using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private GameObject[] layer1;
    [SerializeField]
    private GameObject[] layer2;
    private int mode = 0;
    [SerializeField]
    private GameObject[] faceUserUI;

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
        foreach (var layer in layer2)
            layer.SetActive(!layer.activeInHierarchy);
    }

    public void SetVessels()
    {
        foreach (var layer in layer1)
            layer.SetActive(!layer.activeInHierarchy);
    }

    public void ResetLayers()
    {
        ToggleLayers(0);
    }

    private void ToggleLayers(int mode)
    {
        switch (mode)
        {
            case 0:
                foreach (var layer in layer1)
                    layer.SetActive(false);
                foreach (var layer2 in layer2)
                    layer2.SetActive(false);
                break;
            case 1:
                foreach (var layer in layer1)
                    layer.SetActive(true);
                foreach (var layer2 in layer2)
                    layer2.SetActive(false);
                break;
            case 2:
                foreach (var layer in layer1)
                    layer.SetActive(false);
                foreach (var layer2 in layer2)
                    layer2.SetActive(true);
                break;
            case 3:
                foreach (var layer in layer1)
                    layer.SetActive(true);
                foreach (var layer2 in layer2)
                    layer2.SetActive(true);
                break;
        }

    }
}
