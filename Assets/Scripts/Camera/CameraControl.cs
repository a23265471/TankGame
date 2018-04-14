using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float m_DampTime = 0.2f;                 
    public float m_ScreenEdgeBuffer = 4f;           
    public float m_MinSize = 6.5f;                  
    /*[HideInInspector] */public Transform[] m_Targets; 


    private Camera m_Camera;                        
    private float m_ZoomSpeed;                      
    private Vector3 m_MoveVelocity;                 
    private Vector3 m_DesiredPosition;              


    private void Awake()
    {
        m_Camera = GetComponentInChildren<Camera>();
    }


    private void FixedUpdate()
    {
        Move();
        Zoom();
    }


    private void Move()
    {
        FindAveragePosition();

        transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
    }


    private void FindAveragePosition()
    {
        Vector3 averagePos = new Vector3();
        int numTargets = 0;

        for (int i = 0; i < m_Targets.Length; i++)
        {
            if (!m_Targets[i].gameObject.activeSelf)
                continue;//跳過此次迴圈,進行下一次的迴圈; return跳出function; break跳脫迴圈

            averagePos += m_Targets[i].position;
            numTargets++;
        }

        if (numTargets > 0)
            averagePos /= numTargets;//averagePos除以numTargets

        averagePos.y = transform.position.y;

        m_DesiredPosition = averagePos;
    }


    private void Zoom()
    {
        float requiredSize = FindRequiredSize();
        m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
    }


    private float FindRequiredSize()
    {
        //將目前攝影機所追隨的目標轉換成local座標
        Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);//transform.InverseTransformPoint世界座標轉為子座標

        float size = 0f;

        for (int i = 0; i < m_Targets.Length; i++)//比距離大小,以用來調整攝影機的遠近
        {
            if (!m_Targets[i].gameObject.activeSelf)
                continue;

            Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);//將坦克車的世界座標轉為local坐標系

            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;//得到坦克車距離攝影機的相對距離

            size = Mathf.Max (size, Mathf.Abs (desiredPosToTarget.y));//先以Y軸作條件,取得攝影機該有的size

            size = Mathf.Max (size, Mathf.Abs (desiredPosToTarget.x) / m_Camera.aspect);//再來與X軸比較,取得攝影機該有的size,最後以最大值為結果
        }
        
        size += m_ScreenEdgeBuffer;

        size = Mathf.Max(size, m_MinSize);//鏡頭的最小值

        return size;
    }


    public void SetStartPositionAndSize()
    {
        FindAveragePosition();

        transform.position = m_DesiredPosition;

        m_Camera.orthographicSize = FindRequiredSize();
    }
}