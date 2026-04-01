using Deepwave.ICR.Editor.Export;
using UnityEditor;
using UnityEngine;

namespace Deepwave.ICR.Editor
{
    [CustomEditor(typeof(InfiniteCorrugatedRoof))]
    public class InfiniteCorrugatedRoofEditor : UnityEditor.Editor
    {
        private SerializedProperty p_panelWidth;
        private SerializedProperty p_panelLength;
        private SerializedProperty p_verticalOverlap;
        private SerializedProperty p_overlapDistance;
        private SerializedProperty p_columnCount;
        private SerializedProperty p_rowCount;
        private SerializedProperty p_waveProfile;
        private SerializedProperty p_wavesPerPanel;
        private SerializedProperty p_corrugationAmplitude;
        private SerializedProperty p_lengthSegmentsPerPanel;
        private SerializedProperty p_sheetThickness;
        private SerializedProperty p_panelTransform;
        private SerializedProperty p_noise;
        private SerializedProperty p_spline;
        private SerializedProperty p_cuts;
        private SerializedProperty p_textureMapping;
        private SerializedProperty p_useBakedMesh;
        private SerializedProperty p_randomSeed;
        private SerializedProperty p_bakedFbxAsset;

        private static bool f_dimensions = true;
        private static bool f_wave = true;
        private static bool f_mesh = false;
        private static bool f_modifiers = false;
        private static bool f_render = false;
        private static bool f_system = true;
        private bool _bakeApplyFailed = false;

        private string _deferredBakePath = null;
        private bool _deferredBakeCreateAsset = false;

        private void OnEnable()
        {
            p_panelWidth = serializedObject.FindProperty("panelWidth");
            p_panelLength = serializedObject.FindProperty("panelLength");
            p_verticalOverlap = serializedObject.FindProperty("verticalOverlap");
            p_overlapDistance = serializedObject.FindProperty("overlapDistance");
            p_columnCount = serializedObject.FindProperty("columnCount");
            p_rowCount = serializedObject.FindProperty("rowCount");

            p_waveProfile = serializedObject.FindProperty("waveProfile");
            p_wavesPerPanel = serializedObject.FindProperty("wavesPerPanel");
            p_corrugationAmplitude = serializedObject.FindProperty("corrugationAmplitude");

            p_lengthSegmentsPerPanel = serializedObject.FindProperty("lengthSegmentsPerPanel");
            p_sheetThickness = serializedObject.FindProperty("sheetThickness");

            p_panelTransform = serializedObject.FindProperty("panelTransform");
            p_noise = serializedObject.FindProperty("noise");
            p_spline = serializedObject.FindProperty("spline");
            p_cuts = serializedObject.FindProperty("cuts");

            p_textureMapping = serializedObject.FindProperty("textureMapping");

            p_useBakedMesh = serializedObject.FindProperty("useBakedMesh");
            p_randomSeed = serializedObject.FindProperty("randomSeed");
            p_bakedFbxAsset = serializedObject.FindProperty("bakedFbxAsset");
        }

        public override void OnInspectorGUI()
        {
            InfiniteCorrugatedRoof sheet = (InfiniteCorrugatedRoof)target;
            serializedObject.Update();

            _deferredBakePath = null;
            _deferredBakeCreateAsset = false;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Infinite Corrugated Roof", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawSection("Panel Layout & Dimensions", ref f_dimensions, () =>
            {
                EditorGUILayout.PropertyField(p_columnCount, new GUIContent("Panel Columns (X)"));
                EditorGUILayout.PropertyField(p_rowCount, new GUIContent("Panel Rows (Z)"));

                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(p_panelWidth, new GUIContent("Panel Width (m)"));
                EditorGUILayout.PropertyField(p_panelLength, new GUIContent("Panel Length (m)"));
                EditorGUILayout.PropertyField(p_verticalOverlap, new GUIContent("Row Overlap (m)"));
                EditorGUILayout.PropertyField(p_overlapDistance, new GUIContent("Layer Gap (m)"));
            });

            DrawSection("Wave Configuration", ref f_wave, () =>
            {
                EditorGUILayout.PropertyField(p_waveProfile, new GUIContent("Wave Profile"));
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(p_wavesPerPanel, new GUIContent("Waves/Panel"));
                EditorGUILayout.PropertyField(p_corrugationAmplitude, new GUIContent("Amplitude"));
            });

            DrawSection("Mesh Settings", ref f_mesh, () =>
            {
                EditorGUILayout.PropertyField(p_lengthSegmentsPerPanel, new GUIContent("Segments per Panel"));
                EditorGUILayout.PropertyField(p_sheetThickness, new GUIContent("Thickness"));
            });

            DrawSection("Modifiers & Variations", ref f_modifiers, () =>
            {
                DrawSubSettings(p_panelTransform, "Panel Transform");
                EditorGUILayout.Space(5);
                DrawSubSettings(p_noise, "Surface Noise");
                EditorGUILayout.Space(5);
                DrawSplineSettings(p_spline);
                EditorGUILayout.Space(5);
                DrawSubSettings(p_cuts, "Cut Settings");
            });

            DrawSection("Rendering & Material", ref f_render, () =>
            {
                DrawSubSettings(p_textureMapping, "Texture Mapping");
            });

            DrawSection("System & Export", ref f_system, () =>
            {
                EditorGUILayout.PropertyField(p_randomSeed, new GUIContent("Random Seed"));
                EditorGUILayout.Space(10);
                DrawBakeUI(sheet);
            });

            bool changed = serializedObject.ApplyModifiedProperties();

            if (!string.IsNullOrEmpty(_deferredBakePath))
            {
                string path = _deferredBakePath;
                bool createAsset = _deferredBakeCreateAsset;
                _deferredBakePath = null;

                ICFFbxExporter.BakeToPath(sheet, path);
                
                if (createAsset)
                {
                    AssetDatabase.ImportAsset(path);
                    p_bakedFbxAsset.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    serializedObject.ApplyModifiedProperties();
                }

                if (sheet.useBakedMesh) _bakeApplyFailed = !sheet.TryApplyBakedMeshes();

                GUIUtility.ExitGUI();
                return;
            }

            if (changed && !sheet.useBakedMesh) sheet.Rebuild();
        }

        private void DrawSubSettings(SerializedProperty prop, string label)
        {
            EditorGUILayout.PropertyField(prop, new GUIContent(label), true);
        }

        private void DrawSplineSettings(SerializedProperty prop)
        {
#if ICR_SPLINES_SUPPORTED
            EditorGUILayout.PropertyField(prop, new GUIContent("Spline Path"), true);
#else
            EditorGUILayout.LabelField("Spline Path (Unavailable)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("The Splines package is not installed.", MessageType.Info);
#endif
        }

        private void DrawSection(string title, ref bool isExpanded, System.Action drawContent)
        {
            GUIStyle boxStyle = new GUIStyle(EditorStyles.helpBox);
            boxStyle.padding = new RectOffset(10, 10, 5, 10);
            boxStyle.margin = new RectOffset(0, 0, 5, 5);

            EditorGUILayout.BeginVertical(boxStyle);
            try
            {
                GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold, fontSize = 11 };
                isExpanded = EditorGUILayout.Foldout(isExpanded, title, true, foldoutStyle);

                if (isExpanded)
                {
                    EditorGUILayout.Space(5);
                    drawContent.Invoke();
                }
            }
            finally
            {
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawBakeUI(InfiniteCorrugatedRoof sheet)
        {
            EditorGUILayout.LabelField("Baking Tools", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(p_bakedFbxAsset, new GUIContent("Target Asset"));

            if (GUILayout.Button("New", GUILayout.Width(45)))
            {
                string path = EditorUtility.SaveFilePanelInProject("Create FBX", "ICF_Baked", "fbx", "Save baked mesh");
                if (!string.IsNullOrEmpty(path))
                {
                    _deferredBakePath = path;
                    _deferredBakeCreateAsset = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            bool hasBakedAsset = p_bakedFbxAsset.objectReferenceValue != null;
            EditorGUI.BeginDisabledGroup(!hasBakedAsset);
            EditorGUILayout.PropertyField(p_useBakedMesh, new GUIContent("Preview Baked Mesh"));
            EditorGUI.EndDisabledGroup();

            if (p_useBakedMesh.boolValue && _bakeApplyFailed)
                EditorGUILayout.HelpBox("Could not load Mesh from the assigned Asset.", MessageType.Warning);

            EditorGUILayout.Space(5);
            EditorGUI.BeginDisabledGroup(!hasBakedAsset);
            if (GUILayout.Button("Bake Geometry to Asset", GUILayout.Height(25)))
            {
                _deferredBakePath = AssetDatabase.GetAssetPath(p_bakedFbxAsset.objectReferenceValue);
                _deferredBakeCreateAsset = false;
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}