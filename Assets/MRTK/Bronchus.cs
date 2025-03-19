using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bronchus : MonoBehaviour
{
    [SerializeField]
    private GameObject[] toggleOnHover;
    [SerializeField]
    private HUDController hud;
    [SerializeField]
    private Material[] testMaterials;
    [SerializeField]
    private Renderer vessels;

    // TODO:
    // - show rotation axis indicator
    // - show select toggle options?
    // - Show anatomical names?
    public void OnHover()
    {
        foreach (var item in toggleOnHover)
        {
            item.transform.GetChild(0).localScale = Vector3.zero;
            item.SetActive(true);
            LeanTween.scale(item.transform.GetChild(0).gameObject, Vector3.one, 0.5f).setEaseInOutElastic();
        }
    }

    public void OnHoverEnd()
    {
        foreach (var item in toggleOnHover)
            item.SetActive(false);
    }

    public void OnTest()
    {

    }
    public void OnReset()
    {
        Camera mainCamera = Camera.main;
        transform.SetPositionAndRotation(mainCamera.transform.position + mainCamera.transform.forward, Quaternion.identity);
        transform.localScale = Vector3.one;
        transform.LookAt(mainCamera.transform);
        hud.ResetLayers();
    }
}
