using UnityEngine;

public class WorldPromptTarget : MonoBehaviour
{
    [SerializeField] private Vector3 screenOffset = new Vector3(0f, 1.5f, 0f);

    public Vector3 GetPromptWorldPosition()
    {
        return transform.position + screenOffset;
    }
}