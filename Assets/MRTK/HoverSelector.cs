using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverSelector : MonoBehaviour
{
    [SerializeField]
    private GameObject[] toggleOnHover;
    
    // TODO:
    // - show rotation axis indicator
    // - show select toggle options?
    // - Show anatomical names?
    public void OnHover()
    {
        foreach (var item in toggleOnHover)
        {
            item.SetActive(true);
        }
    }

    public void OnHoverEnd()
    {
        foreach (var item in toggleOnHover)
        {
            item.SetActive(false);
        }
    }
}
