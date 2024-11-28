using UnityEngine;

namespace Complete
{
    public class CameraControl : MonoBehaviour
    {
        public float m_DampTime = 0.2f;                 // ī�޶� �缳�� �ð� �ٻ簪.
        public float m_ScreenEdgeBuffer = 4f;           // Space between the top/bottom most target and the screen edge.
        public float m_MinSize = 6.5f;                  // ī�޶� �������� �ּҰ�.
        [HideInInspector] public Transform[] m_Targets; // All the targets the camera needs to encompass.

        private Camera m_Camera;                        // Used for referencing the camera.
        private float m_ZoomSpeed;                      // Reference speed for the smooth damping of the orthographic size.
        private Vector3 m_MoveVelocity;                 // Reference velocity for the smooth damping of the position.
        private Vector3 m_DesiredPosition;              // The position the camera is moving towards.


        private void Awake()
        {
            m_Camera = GetComponentInChildren<Camera>();
        }


        private void FixedUpdate()
        {
            // ī�޶� �߰� �������� �̵�
            Move();
            // ī�޶��� �� ��ġ ����
            Zoom();
        }


        private void Move()
        {
            // �� ��ũ�� �߰��� ã��
            FindAveragePosition();

            // �ε巯�� �̵� ����
            transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
        }


        private void FindAveragePosition()
        {
            Vector3 averagePos = new Vector3();
            int numTargets = 0;

            // ��� Ÿ�� ���� ȹ��
            for (int i = 0; i < m_Targets.Length; i++)
            {
                // Ÿ���� Ȱ��ȭ�Ǿ� ���� ������ �������� �̵�.
                if (m_Targets[i] == null || !m_Targets[i].gameObject.activeSelf)
                    continue;

                // ��� ���� ����ϱ� ���� ��� ������ ���� ����.
                averagePos += m_Targets[i].position;
                numTargets++;
            }

            // ���� ���� ������ ���Ͽ� ������ ��� ����
            if (numTargets > 0)
                averagePos /= numTargets;

            // ���� Y���� �״�� ������.
            averagePos.y = transform.position.y;

            // ����� ��� ���� ���� �������� ����;
            m_DesiredPosition = averagePos;
        }


        private void Zoom()
        {
            // �� ��ũ�� ���̴� ������ ���� ã�� ī�޶��� �� �ε巴�� ����.
            float requiredSize = FindRequiredSize();
            m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
        }


        private float FindRequiredSize()
        {
            // ��� ������� ������ ���� ���� ��ǥ���� ī�޶��� ���� ��ǥ�� ��ȯ.
            Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);

            // ������ 0���� ����
            float size = 0f;

            // ��� Ÿ�� Ž��
            for (int i = 0; i < m_Targets.Length; i++)
            {
                // ��Ƽ�� �Ǿ����� �ʰų� ���� ��� �Ѿ
                if (m_Targets[i] == null || !m_Targets[i].gameObject.activeSelf)
                    continue;

                // ī�޶� ���� �����̽��� �������� ����� �������� ã��
                Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);

                // ī�޶� ���� ��ǥ�� �������� �ؼ�, ��� ��յ� ������ ������� ���ϴ� ���͸� ����
                Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

                // Ÿ���� ī�޶� ���� �󸶳� �� �Ǵ� �Ʒ��� �ִ����� Ȯ��, size ������ ������ size ���� �� �Ÿ��� �ִ����� ������Ʈ
                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

                //  Ÿ���� ���� ��ġ�� ī�޶��� ��Ⱦ�� ���� �󸶳� �����ϴ����� ���, size�� ���Ͽ� �ִ����� ������Ʈ
                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);
            }

            // �׵θ� ����
            size += m_ScreenEdgeBuffer;

            // �ּڰ� ����
            size = Mathf.Max(size, m_MinSize);

            return size;
        }


        public void SetStartPositionAndSize()
        {
            // Find the desired position.
            FindAveragePosition();

            // Set the camera's position to the desired position without damping.
            transform.position = m_DesiredPosition;

            // Find and set the required size of the camera.
            m_Camera.orthographicSize = FindRequiredSize();
        }
    }
}