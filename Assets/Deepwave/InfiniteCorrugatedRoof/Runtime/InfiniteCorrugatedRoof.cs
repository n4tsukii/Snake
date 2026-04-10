#if ICR_SPLINES_SUPPORTED
using UnityEngine.Splines;
#endif

using UnityEngine;
using UnityEngine.Serialization;
using Deepwave.ICR.Data;
using Deepwave.ICR.Utilities;
using Deepwave.ICR.Rendering;
using Deepwave.ICR.Core;
using Deepwave.ICR.Generation;

using CurveUtility = Deepwave.ICR.Utilities.CurveUtility;
using SplineUtility = Deepwave.ICR.Integration.SplineUtility;

namespace Deepwave.ICR
{
    [ExecuteAlways]
    [RequireComponent(typeof(LODGroup))]
    public class InfiniteCorrugatedRoof : MonoBehaviour
    {
        #region Configuration Fields
        [FormerlySerializedAs("panelWidthMeters")]
        public float panelWidth = 1.0f;
        [FormerlySerializedAs("panelLengthMeters")]
        public float panelLength = 2.0f;
        [FormerlySerializedAs("rowOverlapMeters")]
        public float verticalOverlap = 0.15f;
        [FormerlySerializedAs("layerGapMeters")]
        public float overlapDistance = 0.001f;
        public int columnCount = 1;
        public int rowCount = 1;

        public AnimationCurve waveProfile = new(
            new Keyframe(0, 0),
            new Keyframe(0.2f, 0),
            new Keyframe(0.5f, 1),
            new Keyframe(0.7f, 1),
            new Keyframe(1, 0)
        );

        public int wavesPerPanel = 9;
        public float corrugationAmplitude = 0.035f;
        public int lengthSegmentsPerPanel = 24;
        public float sheetThickness = 0.001f;

        public CutSettings cuts = CutSettings.Default;
        public NoiseSettings noise = NoiseSettings.Default;
        public SplineSettings spline = SplineSettings.Default;
        public TextureSettings textureMapping = TextureSettings.Default;
        public TransformSettings panelTransform = TransformSettings.Default;

        public bool useBakedMesh = false;
        public int randomSeed = 12345;
        public GameObject bakedFbxAsset;

        [SerializeField, HideInInspector]
        private bool hasInitializedSettings;
        #endregion

        #region Components
        private RoofRenderer _renderer;
        private IRoofGenerator _generator;
        private SplineUtility.SplineCache _splineCache;
        private readonly Mesh[] _proceduralMeshes = new Mesh[LodUtility.LodCount];
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
#if ICR_SPLINES_SUPPORTED
            Spline.Changed += OnSplineChanged;
#endif
            Initialize();
            if (ShouldUseBakedMesh() && TryApplyBakedMeshes()) return;
            Rebuild();
        }

        private void OnDisable()
        {
#if ICR_SPLINES_SUPPORTED
            Spline.Changed -= OnSplineChanged;
#endif
        }

        private void OnDestroy()
        {
            CleanupMeshes();
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            Initialize();
            if (ShouldUseBakedMesh() && TryApplyBakedMeshes()) return;
            Rebuild();
        }
        #endregion

        #region Event Handlers
#if ICR_SPLINES_SUPPORTED
        private void OnSplineChanged(Spline splineRef, int index, SplineModification modification)
        {
            if (splineRef == null || spline.spline == null || spline.spline.Spline != splineRef)
                return;

            if (!ShouldUseBakedMesh()) Rebuild();
        }
#endif
        #endregion

        #region Core Logic
        private void Initialize()
        {
            if (!hasInitializedSettings)
            {
                panelTransform.enabled = true;
                cuts.enabled = true;
                textureMapping.enabled = true;
                hasInitializedSettings = true;
            }

            _renderer ??= new RoofRenderer(transform);
            _generator ??= new DefaultRoofGenerator();
            
            _renderer.Setup(GetComponent<MeshRenderer>());
        }

        public void Rebuild()
        {
            Initialize();
            Sanitize();
            UpdateSplineCache();

            var buildSettings = new BuildSettings(this);
            string instanceId = GetInstanceID().ToString();
            var context = new BuildContext(buildSettings, textureMapping, null, in _splineCache, 0, 0, lengthSegmentsPerPanel);

            for (int i = 0; i < LodUtility.LodCount; i++)
            {
                string proceduralMeshName = $"ICR_LOD{i}_{instanceId}";
                Mesh currentMesh = _renderer.GetMesh(i);

                if (currentMesh != null && currentMesh.name == proceduralMeshName)
                {
                    _proceduralMeshes[i] = currentMesh;
                    _proceduralMeshes[i].Clear();
                }
                else
                {
                    _proceduralMeshes[i] = new Mesh { name = proceduralMeshName };
                    _proceduralMeshes[i].MarkDynamic();
                }

                _generator.Generate(_proceduralMeshes[i], i, context);
            }

            _renderer.ApplyMeshes(_proceduralMeshes);
        }

        private void Sanitize()
        {
            columnCount = SettingsSanitizer.SanitizeMinimum(columnCount, 1);
            rowCount = SettingsSanitizer.SanitizeMinimum(rowCount, 1);
            lengthSegmentsPerPanel = SettingsSanitizer.SanitizeMinimum(lengthSegmentsPerPanel, 1);
            wavesPerPanel = SettingsSanitizer.SanitizeMinimum(wavesPerPanel, 1);

            panelWidth = SettingsSanitizer.SanitizePositive(panelWidth, 1f, SettingsSanitizer.MinPanelSize);
            panelLength = SettingsSanitizer.SanitizePositive(panelLength, 2f, SettingsSanitizer.MinPanelSize);

            verticalOverlap = SettingsSanitizer.SanitizeRowOverlap(verticalOverlap, panelLength);
            overlapDistance = SanitizeNonNegative(overlapDistance, 0.001f);

            corrugationAmplitude = SanitizeNonNegative(corrugationAmplitude, 0.035f);
            sheetThickness = SanitizeNonNegative(sheetThickness, 0.001f);

            panelTransform = SettingsSanitizer.Sanitize(panelTransform, panelLength);
            noise = SettingsSanitizer.Sanitize(noise);
            spline = SettingsSanitizer.Sanitize(spline);
            cuts = SettingsSanitizer.Sanitize(cuts);
            textureMapping = SettingsSanitizer.Sanitize(textureMapping);
            waveProfile = CurveUtility.SanitizeCurve(waveProfile);
        }

        private float SanitizeNonNegative(float val, float fallback) => val < 0 ? fallback : val;

        private void UpdateSplineCache()
        {
#if ICR_SPLINES_SUPPORTED
            if (spline.enabled && spline.spline != null)
                _splineCache = SplineUtility.BuildCache(spline, transform, 0f);
            else
                _splineCache = default;
#else
            _splineCache = default;
#endif
        }

        public bool ShouldUseBakedMesh() => (useBakedMesh || Application.isPlaying) && bakedFbxAsset != null;

        public bool TryApplyBakedMeshes()
        {
            if (bakedFbxAsset == null) return false;

            Mesh[] foundMeshes = new Mesh[LodUtility.LodCount];
            for (int i = 0; i < LodUtility.LodCount; i++)
            {
                var child = bakedFbxAsset.transform.Find($"LOD{i}");
                var meshFilter = child ? child.GetComponent<MeshFilter>() : null;
                if (meshFilter == null || meshFilter.sharedMesh == null) return false;
                foundMeshes[i] = meshFilter.sharedMesh;
            }

            Initialize();
            CleanupMeshes();
            _renderer.ApplyMeshes(foundMeshes);
            return true;
        }

        private void CleanupMeshes()
        {
            for (int i = 0; i < _proceduralMeshes.Length; i++)
            {
                if (_proceduralMeshes[i] != null)
                {
                    if (Application.isPlaying) Destroy(_proceduralMeshes[i]);
                    else DestroyImmediate(_proceduralMeshes[i]);
                    _proceduralMeshes[i] = null;
                }
            }
        }

        public Mesh GetProceduralMesh(int lodIndex) => _proceduralMeshes[lodIndex];
        #endregion
    }
}