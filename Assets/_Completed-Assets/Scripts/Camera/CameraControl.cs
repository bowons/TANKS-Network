using UnityEngine;

namespace Complete
{
    public class CameraControl : MonoBehaviour
    {
        public float m_DampTime = 0.2f;                 // 카메라 재설정 시간 근사값.
        public float m_ScreenEdgeBuffer = 4f;           // Space between the top/bottom most target and the screen edge.
        public float m_MinSize = 6.5f;                  // 카메라 직교투영 최소값.
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
            // 카메라를 중간 지점으로 이동
            Move();
            // 카메라의 줌 수치 조정
            Zoom();
        }


        private void Move()
        {
            // 두 탱크간 중간값 찾음
            FindAveragePosition();

            // 부드러운 이동 적용
            transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
        }


        private void FindAveragePosition()
        {
            Vector3 averagePos = new Vector3();
            int numTargets = 0;

            // 모든 타겟 정보 획득
            for (int i = 0; i < m_Targets.Length; i++)
            {
                // 타겟이 활성화되어 있지 않으면 다음으로 이동.
                if (m_Targets[i] == null || !m_Targets[i].gameObject.activeSelf)
                    continue;

                // 평균 값을 계산하기 위해 모든 포지션 값을 더함.
                averagePos += m_Targets[i].position;
                numTargets++;
            }

            // 벡터 값의 산술평균 구하여 포지션 평균 산출
            if (numTargets > 0)
                averagePos /= numTargets;

            // 기존 Y값을 그대로 유지함.
            averagePos.y = transform.position.y;

            // 산출된 평균 값을 예상 지점으로 설정;
            m_DesiredPosition = averagePos;
        }


        private void Zoom()
        {
            // 두 탱크가 보이는 적절한 값을 찾아 카메라의 줌 부드럽게 조절.
            float requiredSize = FindRequiredSize();
            m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
        }


        private float FindRequiredSize()
        {
            // 산술 평균으로 구해진 값을 월드 좌표에서 카메라의 로컬 좌표로 변환.
            Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);

            // 사이즈 0에서 시작
            float size = 0f;

            // 모든 타겟 탐색
            for (int i = 0; i < m_Targets.Length; i++)
            {
                // 액티브 되어있지 않거나 없을 경우 넘어감
                if (m_Targets[i] == null || !m_Targets[i].gameObject.activeSelf)
                    continue;

                // 카메라 로컬 스페이스를 기준으로 대상의 포지션을 찾음
                Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);

                // 카메라 로컬 좌표를 기준으로 해서, 산술 평균된 값에서 대상으로 향하는 벡터를 구함
                Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

                // 타겟이 카메라에 대해 얼마나 위 또는 아래에 있는지를 확인, size 변수는 현재의 size 값과 이 거리의 최댓값으로 업데이트
                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

                //  타겟의 가로 위치가 카메라의 종횡비에 따라 얼마나 차지하는지를 계산, size와 비교하여 최댓값으로 업데이트
                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);
            }

            // 테두리 보정
            size += m_ScreenEdgeBuffer;

            // 최솟값 보정
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