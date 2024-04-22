using UnityEngine;

public class ShiftScript : MonoBehaviour
{
    private Vector3 startPoint; // 起始点
    private Vector3 endPoint;   // 终点

    public float speed = 1.0f;   // 移动速度

    private float startTime;
    private float journeyLength;

    private bool moving = false;

    // 声明一个委托类型
    public delegate void OnFinishDelegate();

    public event OnFinishDelegate OnFinish;

    void Update()
    {
        if (moving)
        {
            // 计算已经过去的时间比例
            float distCovered = (Time.time - startTime) * speed;
            float fracJourney = distCovered / journeyLength;

            if (fracJourney >= 1.0f)
            {
                moving = false;
                //移动结束
                OnFinish?.Invoke();
            }

            // 在起点和终点之间移动游戏对象
            transform.position = Vector3.Lerp(startPoint, endPoint, fracJourney);
        }
    }

    // 启动移动
    public void StartMove(Vector3 start, Vector3 end)
    {
        startPoint = start;
        endPoint = end;
        // 设置开始时间
        startTime = Time.time;
        journeyLength = Vector3.Distance(startPoint, endPoint);
        moving = true;
    }


}
