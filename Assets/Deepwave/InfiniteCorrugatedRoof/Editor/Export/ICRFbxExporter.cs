#if ICR_FBX_EXPORTER_SUPPORTED
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;

using Deepwave.ICR.Utilities;

namespace Deepwave.ICR.Editor.Export
{
    /// <summary>
    /// Class chịu trách nhiệm duy nhất: Xử lý logic tạo Mesh tạm và Xuất ra file FBX.
    /// </summary>
    public static class ICFFbxExporter
    {
        public static void BakeToPath(InfiniteCorrugatedRoof sheet, string absolutePath)
        {
            if (sheet == null)
            {
                Debug.LogError("[ICF] Bake thất bại: Sheet trống.");
                return;
            }

            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                Debug.LogError("[ICF] Bake thất bại: Đường dẫn trống.");
                return;
            }

            sheet.Rebuild();

            var tempRoot = new GameObject(sheet.name);
            try
            {
                tempRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                tempRoot.transform.localScale = Vector3.one;

                SetupMeshAndLODs(sheet, tempRoot);

                var options = new ExportModelOptions
                {
                    ExportFormat = ExportFormat.Binary
                };

                string relativePath = absolutePath;
                if (Path.IsPathRooted(absolutePath))
                {
                    relativePath = FileUtil.GetProjectRelativePath(absolutePath);
                }

                if (string.IsNullOrWhiteSpace(relativePath))
                {
                    Debug.LogError("[ICF] Bake thất bại: Đường dẫn không hợp lệ.");
                    return;
                }

                ModelExporter.ExportObject(relativePath, tempRoot, options);

                ApplyImportSettings(relativePath);

                Debug.Log($"[ICF] Bake thành công: {relativePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ICF] Bake thất bại: {ex.Message}");
            }
            finally
            {
                if (tempRoot != null)
                {
                    UnityEngine.Object.DestroyImmediate(tempRoot);
                }
            }

            AssetDatabase.Refresh();
        }

        private static void SetupMeshAndLODs(InfiniteCorrugatedRoof sourceSheet, GameObject destinationParent)
        {
            var sourceRenderer = sourceSheet.GetComponent<MeshRenderer>();
            int lodCount = LodUtility.LodCount;

            for (int i = 0; i < lodCount; i++)
            {
                var lodName = $"LOD{i}";
                var lodTransform = sourceSheet.transform.Find(lodName);
                if (lodTransform == null) continue;

                var meshFilter = lodTransform.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null) continue;

                var lodChild = new GameObject(lodName);
                lodChild.transform.SetParent(destinationParent.transform, false);

                var bakedFilter = lodChild.AddComponent<MeshFilter>();
                var bakedRenderer = lodChild.AddComponent<MeshRenderer>();

                var bakedMesh = sourceSheet.GetProceduralMesh(i);
                bakedMesh.name = $"{sourceSheet.name}_{lodName}_{DateTime.Now.Ticks}";

                bakedFilter.sharedMesh = bakedMesh;

                if (lodTransform.TryGetComponent<MeshRenderer>(out var lodRenderer))
                {
                    bakedRenderer.sharedMaterials = lodRenderer.sharedMaterials;
                }
                else if (sourceRenderer != null)
                {
                    bakedRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
                }
            }
        }

        private static void ApplyImportSettings(string relativePath)
        {
            AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);

            if (AssetImporter.GetAtPath(relativePath) is ModelImporter importer)
            {
                importer.importNormals = ModelImporterNormals.Import;
                importer.importTangents = ModelImporterTangents.Import;
                importer.importBlendShapeNormals = ModelImporterNormals.Import;
                importer.SaveAndReimport();
            }
        }
    }
}
#else
using UnityEditor;
using UnityEngine;

using Deepwave.ICR.Utilities;

namespace Deepwave.ICR.Editor.Export
{
    public static class ICFFbxExporter
    {
        public static void BakeToPath(InfiniteCorrugatedRoof sheet, string absolutePath)
        {
            Debug.LogWarning("[ICF] FBX Exporter package is not installed. Please install 'com.unity.formats.fbx' to use this feature.");
            EditorUtility.DisplayDialog("FBX Exporter Missing", "The FBX Exporter package (com.unity.formats.fbx) is not installed. This feature is unavailable.", "OK");
        }
    }
}
#endif