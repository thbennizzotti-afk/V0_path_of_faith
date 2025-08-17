using UnityEngine;
using PathOfFaith.Fondation.Core;

namespace PathOfFaith.Fondation.Exploration
{
    [DisallowMultipleComponent]
    public class CameraIsoController : MonoBehaviour
    {
        [Header("Cible")]
        public Transform target;

        [Header("Suivi (orbite 'corde')")]
        [Range(15f, 75f)] public float pitch = 45f;
        public float baseDistance = 12f;
        public float minDistance = 3f;
        public float maxDistance = 30f;
        public float zoomSpeedFollow = 10f;
        public float orbitSpeed = 3000f;
        public float followLerp = 12f;

        [Header("Libre (top-down)")]
        public float freeMoveSpeed = 20f;
        public float freeZoomSpeed = 20f;
        public float freeStartHeight = 30f;

        [Header("Transitions")]
        public float transitionSpeed = 12f;
        [Tooltip("Snaper la rotation à 90° en conservant le yaw courant (évite tout flip).")]
        public bool snapRotationOnFree = true;

        [Header("Interaction & Filtrage")]
        public float orbitHoldThreshold = 0.15f;
        public float scrollDeadzone   = 0.05f;
        public float orbitDeadzone    = 0.02f;

        [Header("ZQSD (libre) — inversions si besoin")]
        public bool invertHorizontal = false;
        public bool invertVertical   = false;

        [Header("Config (optionnelle)")]
        public ExplorationConfig config;
        public bool useConfig = false; // OFF par défaut

        private enum CamMode { Follow, Free }
        private CamMode _mode = CamMode.Follow;

        private float _yawDeg = 0f;
        private float _distance;
        private bool  _isOrbiting = false;
        private float _holdTimer  = 0f;

        private bool   _transitioning = false;
        private Vector3 _fromPos, _toPos;
        private float _t;

        private bool _initialized = false;
        private float _freeYaw = 0f;
        private Vector2 _lockedViewport = new Vector2(0.5f, 0.5f);

        private Camera _cam;

        private bool IsExplorationActive() =>
            GameStateManager.Instance == null ||
            GameStateManager.Instance.IsExploration();

        private void ApplyConfigIfAny()
        {
            if (!useConfig || config == null) return;

            // Follow
            pitch           = config.pitch;
            baseDistance    = config.baseDistance;
            minDistance     = config.minDistance;
            maxDistance     = config.maxDistance;
            zoomSpeedFollow = config.zoomSpeedFollow;
            orbitSpeed      = config.orbitSpeed;
            followLerp      = config.followLerp;

            // Free
            freeMoveSpeed   = config.freeMoveSpeed;
            freeZoomSpeed   = config.freeZoomSpeed;
            freeStartHeight = config.freeStartHeight;

            // Transitions
            transitionSpeed     = config.transitionSpeed;
            snapRotationOnFree  = config.snapRotationOnFree;

            // Thresholds
            orbitHoldThreshold  = config.orbitHoldThreshold;
            scrollDeadzone      = config.scrollDeadzone;
            orbitDeadzone       = config.orbitDeadzone;
            invertHorizontal    = config.invertHorizontal;
            invertVertical      = config.invertVertical;
        }

        private void Start()
        {
            _cam = Camera.main;
            ApplyConfigIfAny();

            _distance = Mathf.Clamp(baseDistance, minDistance, maxDistance);
            _freeYaw = transform.eulerAngles.y;
        }

        private void Update()
        {
            if (!IsExplorationActive() || target == null) return;

            if (!_initialized)
            {
                SnapToFollow();
                _initialized = true;
                _freeYaw = transform.eulerAngles.y;
            }

            // RMB : bascule Follow <-> Free
            if (Input.GetMouseButtonDown(1))
                BeginTransition(_mode == CamMode.Follow ? CamMode.Free : CamMode.Follow);

            if (_mode == CamMode.Follow)
            {
                if (Input.GetMouseButtonDown(0)) { _isOrbiting = false; _holdTimer = 0f; }
                if (Input.GetMouseButton(0))
                {
                    _holdTimer += Time.deltaTime;
                    if (!_isOrbiting && _holdTimer >= orbitHoldThreshold)
                        _isOrbiting = true;
                }
                if (Input.GetMouseButtonUp(0)) { _isOrbiting = false; _holdTimer = 0f; }
            }
        }

        private void LateUpdate()
        {
            if (!IsExplorationActive() || target == null) return;

            if (_transitioning)
            {
                _t += Time.deltaTime * transitionSpeed;
                transform.position = Vector3.Lerp(_fromPos, _toPos, _t);
                if (_t >= 1f) _transitioning = false;
                return;
            }

            if (_mode == CamMode.Follow) FollowMode();
            else                         FreeMode();
        }

        // ---------- MODE SUIVI ----------
        private void FollowMode()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            bool instantZoom = false;
            if (Mathf.Abs(scroll) > scrollDeadzone)
            {
                _distance = Mathf.Clamp(_distance - scroll * zoomSpeedFollow, minDistance, maxDistance);
                instantZoom = true;
            }

            if (_isOrbiting)
            {
                float dx = Input.GetAxisRaw("Mouse X");
                if (Mathf.Abs(dx) > orbitDeadzone)
                    _yawDeg = Mathf.Repeat(_yawDeg + dx * orbitSpeed * Time.deltaTime, 360f);
            }

            float yaw   = _yawDeg * Mathf.Deg2Rad;
            float pr    = pitch   * Mathf.Deg2Rad;
            float horiz = _distance * Mathf.Cos(pr);
            float hgt   = _distance * Mathf.Sin(pr);

            Vector3 focus = target.position;
            Vector3 orbit = new Vector3(horiz * Mathf.Sin(yaw), hgt, horiz * Mathf.Cos(yaw));
            Vector3 desiredPos = focus + orbit;
            Quaternion desiredRot = Quaternion.LookRotation(focus - desiredPos, Vector3.up);

            if (_isOrbiting || instantZoom)
            {
                transform.position = desiredPos;
                transform.rotation = desiredRot;
            }
            else
            {
                float k = followLerp * Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, desiredPos, k);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, k);
            }
        }

        // ---------- MODE LIBRE (top-down + ZQSD indexés écran) ----------
        private void FreeMode()
        {
            Quaternion yawRot = Quaternion.Euler(0f, _freeYaw, 0f);
            Vector3 fwd   = yawRot * Vector3.forward; // haut d’écran
            Vector3 right = yawRot * Vector3.right;   // droite d’écran

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            if (invertHorizontal) h = -h;
            if (invertVertical)   v = -v;

            Vector3 moveDir = (fwd * v + right * h).normalized;
            transform.position += moveDir * freeMoveSpeed * Time.deltaTime;

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
                transform.position += new Vector3(0f, -scroll * freeZoomSpeed, 0f);

            transform.rotation = Quaternion.Euler(90f, _freeYaw, 0f);
        }

        // ---------- TRANSITIONS ----------
        private void BeginTransition(CamMode targetMode)
        {
            _transitioning = true; _t = 0f;
            _fromPos = transform.position;

            if (targetMode == CamMode.Free)
            {
                if (_cam == null) _cam = Camera.main;

                _lockedViewport = _cam != null
                    ? _cam.WorldToViewportPoint(target.position)
                    : new Vector2(0.5f, 0.5f);

                _freeYaw = transform.eulerAngles.y;

                if (snapRotationOnFree)
                    transform.rotation = Quaternion.Euler(90f, _freeYaw, 0f);

                float H = freeStartHeight - target.position.y;
                if (_cam == null) _cam = Camera.main;
                float fovRad = (_cam != null ? _cam.fieldOfView : 60f) * Mathf.Deg2Rad;
                float halfH = Mathf.Tan(fovRad * 0.5f) * Mathf.Abs(H);
                float halfW = halfH * (_cam != null ? _cam.aspect : 16f/9f);

                float du = (_lockedViewport.x - 0.5f) * 2f;
                float dv = (_lockedViewport.y - 0.5f) * 2f;
                float x_cam = du * halfW;
                float y_cam = dv * halfH;

                Vector3 right = transform.right;
                Vector3 upH   = transform.up;

                Vector3 basePos = target.position + right * x_cam + upH * y_cam;
                _toPos = new Vector3(basePos.x, target.position.y + H, basePos.z);

                _mode = CamMode.Free;
                EventBus.Raise(new Events.CamModeChanged(Events.CamModeChanged.Mode.Free));
            }
            else
            {
                float yaw   = _yawDeg * Mathf.Deg2Rad;
                float pr    = pitch   * Mathf.Deg2Rad;
                float horiz = _distance * Mathf.Cos(pr);
                float hgt   = _distance * Mathf.Sin(pr);
                Vector3 f   = target.position;
                Vector3 orbit = new Vector3(horiz * Mathf.Sin(yaw), hgt, horiz * Mathf.Cos(yaw));
                _toPos = f + orbit;

                transform.rotation = Quaternion.LookRotation(f - _toPos, Vector3.up);
                _mode = CamMode.Follow;
                EventBus.Raise(new Events.CamModeChanged(Events.CamModeChanged.Mode.Follow));
            }
        }

        // ---------- UTILS ----------
        private void SnapToFollow()
        {
            float yaw   = _yawDeg * Mathf.Deg2Rad;
            float pr    = pitch   * Mathf.Deg2Rad;
            float horiz = _distance * Mathf.Cos(pr);
            float hgt   = _distance * Mathf.Sin(pr);

            Vector3 f = target.position;
            Vector3 orbit = new Vector3(horiz * Mathf.Sin(yaw), hgt, horiz * Mathf.Cos(yaw));
            Vector3 p = f + orbit;

            transform.position = p;
            transform.rotation = Quaternion.LookRotation(f - p, Vector3.up);
        }
    }
}
