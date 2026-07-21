using UnityEngine;

public class KeyboardArea : MonoBehaviour
{
    public float width = 3.8f;
    public float height = 1.4f;

    public Vector3 GetRandomPoint()
    {
        float x = Random.Range(-width / 2f, width / 2f);
        float y = Random.Range(-height / 2f, height / 2f);

        return transform.TransformPoint(new Vector3(x, y, 0));
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireCube(Vector3.zero,
            new Vector3(width, height, 0));
    }
#endif
}