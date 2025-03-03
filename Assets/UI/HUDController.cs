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

    //TODO:
    // - reset button above gizmo
    //      -> should reset position, rotation and scale
    // - less/more detail arrow on each side of gizmo
    private void Update()
    {
        transform.position = trackedObject.position;
        orientationGizmo.rotation = trackedObject.rotation;
    }

    public void EnableOrientationGizmo()
    {
        gizmoMesh.enabled = true;
    }

    public void DisableOrientationGizmo()
    {
        gizmoMesh.enabled = false;
    }
}
