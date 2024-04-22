using UnityEngine;

public class ShiftScript : MonoBehaviour
{
    private Vector3 startPoint; // ��ʼ��
    private Vector3 endPoint;   // �յ�

    public float speed = 1.0f;   // �ƶ��ٶ�

    private float startTime;
    private float journeyLength;

    private bool moving = false;

    // ����һ��ί������
    public delegate void OnFinishDelegate();

    public event OnFinishDelegate OnFinish;

    void Update()
    {
        if (moving)
        {
            // �����Ѿ���ȥ��ʱ�����
            float distCovered = (Time.time - startTime) * speed;
            float fracJourney = distCovered / journeyLength;

            if (fracJourney >= 1.0f)
            {
                moving = false;
                //�ƶ�����
                OnFinish?.Invoke();
            }

            // �������յ�֮���ƶ���Ϸ����
            transform.position = Vector3.Lerp(startPoint, endPoint, fracJourney);
        }
    }

    // �����ƶ�
    public void StartMove(Vector3 start, Vector3 end)
    {
        startPoint = start;
        endPoint = end;
        // ���ÿ�ʼʱ��
        startTime = Time.time;
        journeyLength = Vector3.Distance(startPoint, endPoint);
        moving = true;
    }


}
