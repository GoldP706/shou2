using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
public class QuadLiquidWobble : MonoBehaviour
{
    [Header("【晃动幅度】卡通推荐 0.3~0.7")]
    public float wobblePower = 0.45f;

    [Header("【晃动速度】晃得快慢")]
    public float wobbleSpeed = 8f;

    [Header("【衰减】0.98=慢慢停，0.95=停得快")]
    public float damping = 0.975f;

    [Header("【泼洒】")]
    public float spillThreshold = 1.2f;
    public float spillInterval = 0.25f;
    public GameObject spillParticlePrefab;
    public GameObject spillStainPrefab;

    private Mesh mesh;
    private Vector3[] verts;
    private float leftH, rightH;   // 左右顶部顶点的高度偏移
    private float leftVel, rightVel;
    private Vector3 lastPos;
    private float spillTimer;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        verts = mesh.vertices;
        lastPos = transform.position;
    }

    void Update()
    {
        if (!Application.isPlaying) return;

        // 1. 计算移动速度
        Vector3 velocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;

        // 2. 速度决定倾斜目标
        float targetTilt = -velocity.x * wobblePower;

        // 3. 弹簧震荡：左右液面像弹簧一样追目标
        leftVel += (targetTilt - leftH) * wobbleSpeed * Time.deltaTime;
        leftVel *= damping;
        leftH += leftVel * Time.deltaTime;

        rightVel += (-targetTilt - rightH) * wobbleSpeed * Time.deltaTime;
        rightVel *= damping;
        rightH += rightVel * Time.deltaTime;

        // 4. 修改Quad顶部两个顶点的高度
        // Quad的4个顶点顺序：0左下, 1左上, 2右下, 3右上
        verts[1].y = 0.5f + leftH;   // 左上
        verts[3].y = 0.5f + rightH;  // 右上

        mesh.vertices = verts;
        mesh.RecalculateBounds();

        // 5. 倾斜超阈值就泼
        spillTimer += Time.deltaTime;
        float tiltAmount = Mathf.Abs(leftH - rightH);
        if (tiltAmount > spillThreshold && spillTimer >= spillInterval)
        {
            DoSpill(velocity.x);
            spillTimer = 0f;
        }
    }

    void DoSpill(float speedX)
    {
        // 从高的一侧泼出去
        float side = speedX < 0 ? 1f : -1f;
        float highY = speedX < 0 ? rightH : leftH;
        Vector3 spillPos = transform.TransformPoint(side * 0.5f, 0.5f + highY, 0);

        if (spillParticlePrefab != null)
            Instantiate(spillParticlePrefab, spillPos, Quaternion.identity);

        if (spillStainPrefab != null)
            StartCoroutine(SpawnStain(spillPos, speedX));
    }

    System.Collections.IEnumerator SpawnStain(Vector3 startPos, float speedX)
    {
        yield return new WaitForSeconds(0.45f);
        Vector3 landPos = startPos + new Vector3(speedX * 0.12f, -0.9f, 0);
        Instantiate(spillStainPrefab, landPos, Quaternion.identity);
    }

    // 退出运行时还原Quad形状
    void OnDisable()
    {
        if (verts != null && verts.Length >= 4)
        {
            verts[1].y = 0.5f;
            verts[3].y = 0.5f;
            mesh.vertices = verts;
        }
    }
}