using System.Collections.Generic;
using UnityEngine;

namespace CoralCritter
{
    /// <summary>
    /// CC-002: Fades blocking geometry between camera and tracked target.
    /// Attach to the same camera object as <see cref="IsometricCameraController"/>.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(IsometricCameraController))]
    public class CameraOcclusionController : MonoBehaviour
    {
        [SerializeField] private LayerMask occluderLayers = ~0;
        [SerializeField, Min(0f)] private float sphereCastRadius = 0.4f;
        [SerializeField, Range(0f, 1f)] private float fadedAlpha = 0.3f;
        [SerializeField] private float fadeInSpeed = 6f;
        [SerializeField] private float fadeOutSpeed = 10f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private readonly Dictionary<Renderer, OccluderState> _occluders = new();
        private readonly HashSet<Renderer> _currentFrameHits = new();
        private readonly RaycastHit[] _hits = new RaycastHit[32];

        private IsometricCameraController _cameraController;
        private MaterialPropertyBlock _propertyBlock;

        private void Awake()
        {
            _cameraController = GetComponent<IsometricCameraController>();
            _propertyBlock = new MaterialPropertyBlock();
        }

        private void LateUpdate()
        {
            _currentFrameHits.Clear();
            var target = _cameraController.Target;
            if (target != null)
            {
                var from = transform.position;
                var to = target.position;
                var direction = to - from;
                var distance = direction.magnitude;

                if (distance > 0.01f)
                {
                    var hitCount = Physics.SphereCastNonAlloc(
                        from,
                        sphereCastRadius,
                        direction.normalized,
                        _hits,
                        distance,
                        occluderLayers,
                        QueryTriggerInteraction.Ignore);

                    for (var i = 0; i < hitCount; i++)
                    {
                        var renderer = _hits[i].collider.GetComponentInParent<Renderer>();
                        if (renderer == null)
                        {
                            continue;
                        }

                        if (!_occluders.ContainsKey(renderer))
                        {
                            _occluders[renderer] = new OccluderState(renderer);
                        }

                        _currentFrameHits.Add(renderer);
                    }
                }
            }

            var keys = new List<Renderer>(_occluders.Keys);
            foreach (var renderer in keys)
            {
                var state = _occluders[renderer];
                var shouldFade = _currentFrameHits.Contains(renderer);
                var targetAlpha = shouldFade ? fadedAlpha : state.OriginalColor.a;
                var speed = shouldFade ? fadeInSpeed : fadeOutSpeed;
                state.CurrentAlpha = Mathf.MoveTowards(state.CurrentAlpha, targetAlpha, speed * Time.deltaTime);

                ApplyAlpha(renderer, state, state.CurrentAlpha);

                if (!shouldFade && Mathf.Approximately(state.CurrentAlpha, state.OriginalColor.a))
                {
                    _occluders.Remove(renderer);
                }
            }
        }

        private void ApplyAlpha(Renderer renderer, OccluderState state, float alpha)
        {
            var color = state.OriginalColor;
            color.a = alpha;

            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(state.ColorPropertyId, color);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        private sealed class OccluderState
        {
            public Color OriginalColor { get; }
            public int ColorPropertyId { get; }
            public float CurrentAlpha { get; set; }

            public OccluderState(Renderer renderer)
            {
                var material = renderer.sharedMaterial;
                if (material != null && material.HasProperty(BaseColorId))
                {
                    ColorPropertyId = BaseColorId;
                    OriginalColor = material.GetColor(BaseColorId);
                }
                else
                {
                    ColorPropertyId = ColorId;
                    OriginalColor = material != null && material.HasProperty(ColorId)
                        ? material.GetColor(ColorId)
                        : Color.white;
                }

                CurrentAlpha = OriginalColor.a;
            }
        }
    }
}
