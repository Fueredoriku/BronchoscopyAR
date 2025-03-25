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
    Vector3 oldPosition;
    [SerializeField, Range(0f, 1f)]
    private float normalizedPathPosition = 0f;
    public float NormalizedPathPosition
    {
        get { return normalizedPathPosition; } set { normalizedPathPosition = Mathf.Clamp(0f, 1f, value); } 
    }
    private void Start()
    {
        travelPath = path.SampledPath.ToArray();
        transform.SetLocalPositionAndRotation(path.positionOffset, Quaternion.Euler(path.rotationOffset));
        oldPosition = travelPath[index];
    }
    void Update()
    {
        index = Mathf.RoundToInt(Mathf.Lerp(1, travelPath.Length - 2, normalizedPathPosition));
        Vector3 currentPosition = -travelPath[index];
        oldPosition = -travelPath[index - 1];
        Vector3 nextPosition = -travelPath[index + 1];
        transform.GetChild(0).localPosition = currentPosition;
        //TODO: oldPosition-currentposition MUST be rotated relative to the transform!!
        Vector3 position = transform.parent.parent.rotation * Vector3.Normalize(oldPosition + currentPosition);
        Debug.DrawRay(transform.position, Vector3.Normalize(oldPosition + currentPosition), Color.red);
        Debug.DrawRay(transform.position, position, Color.green);
        transform.GetChild(0).rotation = transform.parent.parent.rotation * Quaternion.LookRotation(Vector3.Normalize((oldPosition + currentPosition+nextPosition)/3f), Vector3.up);
    }
}
