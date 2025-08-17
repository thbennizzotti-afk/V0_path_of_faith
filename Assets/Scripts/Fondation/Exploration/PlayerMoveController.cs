using UnityEngine;
using PathOfFaith.Fondation.Navigation;
using PathOfFaith.Fondation.Core;

namespace PathOfFaith.Fondation.Exploration
{
    [DisallowMultipleComponent]
    public class PlayerMoveController : MonoBehaviour
    {
        [Header("Réglages (locaux / overrides par config si ON)")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Références")]
        [SerializeField] private NavigationService navigationService; // résolu auto si null
        private ICursorService cursorService;                         // via ServiceLocator

        [Header("Click-to-move (locaux / overrides par config si ON)")]
        [Tooltip("Durée min (s) pour considérer un hold/drag (caméra) au lieu d’un clic court (move).")]
        [SerializeField, Min(0f)] private float clickHoldThreshold = 0.15f;
        [Tooltip("Seuil (px) de déplacement souris au-delà duquel on considère un drag.")]
        [SerializeField, Min(0f)] private float dragPixelThreshold = 6f;

        [Header("Config (optionnelle)")]
        [SerializeField] private bool useConfig = false;
        [SerializeField] private ExplorationConfig config;

        // Runtime
        private Vector3 targetPosition;
        private bool isMoving;

        private bool pendingClick;
        private float mouseDownTime;
        private Vector2 mouseDownScreenPos;
        private bool cancelledByHoldOrDrag;

        // Autoriser le click-to-move seulement en mode Suivi (abonné à l'EventBus)
        private bool allowClickToMove = true;

        // --------- Lifecycle ----------
        private void OnEnable()
        {
            EventBus.OnCamModeChanged += HandleCamModeChanged;

            // Appliquer la config SI activée
            ApplyConfigIfAny();

            // Auto-résoudre les services si non assignés
            if (navigationService == null)
            {
                if (!ServiceLocator.TryGet(out navigationService))
#if UNITY_2023_1_OR_NEWER
                    navigationService = FindFirstObjectByType<NavigationService>();
#else
                    navigationService = FindObjectOfType<NavigationService>();
#endif
            }
            ServiceLocator.TryGet(out cursorService);

            targetPosition = transform.position;
        }

        private void OnDisable()
        {
            EventBus.OnCamModeChanged -= HandleCamModeChanged;
        }

        private void HandleCamModeChanged(Events.CamModeChanged e)
        {
            allowClickToMove = (e.NewMode == Events.CamModeChanged.Mode.Follow);
        }

        private bool IsExplorationActive() =>
            GameStateManager.Instance == null ||
            GameStateManager.Instance.IsExploration();

        // --------- Update loop ----------
        private void Update()
        {
            if (!IsExplorationActive()) return;

            UpdateHoverCursor();
            HandleClickToMove();
            MovePlayer();
        }

        // --------- Config ----------
        private void ApplyConfigIfAny()
        {
            if (!useConfig || config == null) return;

            moveSpeed          = config.playerMoveSpeed;
            clickHoldThreshold = config.clickHoldThreshold;
            dragPixelThreshold = config.dragPixelThreshold; // float OK, on compare en pixels
        }

        // --------- UI/Feedback ----------
        private void UpdateHoverCursor()
        {
            if (pendingClick) return;

            if (navigationService != null && navigationService.TryGetClickNavPoint(out _))
                cursorService?.SetExploreMove();
            else
                cursorService?.SetDefault();
        }

        // --------- Input / Click-to-move ----------
        private void HandleClickToMove()
        {
            if (!allowClickToMove) return;

            if (Input.GetMouseButtonDown(0))
            {
                pendingClick = true;
                cancelledByHoldOrDrag = false;
                mouseDownTime = Time.unscaledTime;
                mouseDownScreenPos = Input.mousePosition;
                return;
            }

            if (pendingClick && Input.GetMouseButton(0))
            {
                float held = Time.unscaledTime - mouseDownTime;
                float pixelDelta = (((Vector2)Input.mousePosition) - mouseDownScreenPos).magnitude;

                if (held >= clickHoldThreshold || pixelDelta >= dragPixelThreshold)
                    cancelledByHoldOrDrag = true;

                return;
            }

            if (pendingClick && Input.GetMouseButtonUp(0))
            {
                if (!cancelledByHoldOrDrag)
                {
                    if (navigationService != null && navigationService.TryGetClickNavPoint(out Vector3 point))
                    {
                        targetPosition = point;
                        isMoving = true;
                        cursorService?.SetExploreMove();

                        EventBus.Raise(new Events.NavPointSelected(point));
                        EventBus.Raise(new Events.PlayerMoveStarted(point));
                    }
                }

                pendingClick = false;
                cancelledByHoldOrDrag = false;
            }
        }

        private void StopMoveAndNotify()
        {
            if (!isMoving) return;
            isMoving = false;
            EventBus.Raise(new Events.PlayerMoveEnded(transform.position));
        }

        private void MovePlayer()
        {
            if (!isMoving) return;

            Vector3 toTarget = targetPosition - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude <= 0.01f) { StopMoveAndNotify(); return; }

            Vector3 direction = toTarget.normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                StopMoveAndNotify();
            }
        }
    }
}
