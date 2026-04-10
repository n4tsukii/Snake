using Deepwave.ICR.Utilities;
using UnityEngine;

namespace Deepwave.ICR.Rendering
{
    internal class RoofRenderer
    {
        private readonly Transform _owner;
        private readonly MeshFilter[] _lodFilters = new MeshFilter[LodUtility.LodCount];
        private readonly MeshRenderer[] _lodRenderers = new MeshRenderer[LodUtility.LodCount];
        private LODGroup _lodGroup;

        public RoofRenderer(Transform owner)
        {
            _owner = owner;
        }

        public void Setup(MeshRenderer mainRenderer)
        {
            if (_lodGroup == null && !_owner.TryGetComponent(out _lodGroup))
                _lodGroup = _owner.gameObject.AddComponent<LODGroup>();

            if (mainRenderer != null && mainRenderer.enabled) mainRenderer.enabled = false;

            bool structureChanged = false;
            var lods = new LOD[LodUtility.LodCount];

            for (int i = 0; i < LodUtility.LodCount; i++)
            {
                var screenRelativeTransitionHeight = LodUtility.GetScreenRelativeHeight(i);
                if (_lodFilters[i] != null && _lodRenderers[i] != null)
                {
                    UpdateLodMaterial(i, mainRenderer);
                    lods[i] = new LOD(screenRelativeTransitionHeight, new Renderer[] { _lodRenderers[i] });
                    continue;
                }

                structureChanged = true;
                string childName = $"LOD{i}";
                Transform child = _owner.Find(childName);

                if (child == null)
                {
                    child = new GameObject(childName).transform;
                    child.SetParent(_owner, false);
                }

                child.gameObject.layer = _owner.gameObject.layer;
                child.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                _lodFilters[i] = child.GetComponent<MeshFilter>();
                _lodRenderers[i] = child.GetComponent<MeshRenderer>();

                if (_lodFilters[i] == null) _lodFilters[i] = child.gameObject.AddComponent<MeshFilter>();
                if (_lodRenderers[i] == null) _lodRenderers[i] = child.gameObject.AddComponent<MeshRenderer>();

                UpdateLodMaterial(i, mainRenderer);
                lods[i] = new LOD(screenRelativeTransitionHeight, new Renderer[] { _lodRenderers[i] });
            }

            if (structureChanged || _lodGroup.GetLODs().Length == 0)
            {
                _lodGroup.SetLODs(lods);
                _lodGroup.RecalculateBounds();
            }
        }

        private void UpdateLodMaterial(int index, MeshRenderer mainRenderer)
        {
            if (mainRenderer != null && _lodRenderers[index] != null)
                _lodRenderers[index].sharedMaterials = mainRenderer.sharedMaterials;
        }

        public void ApplyMeshes(Mesh[] meshes)
        {
            for (int i = 0; i < LodUtility.LodCount; i++)
            {
                if (_lodFilters[i] != null) _lodFilters[i].sharedMesh = meshes[i];
            }
            if (_lodGroup != null) _lodGroup.RecalculateBounds();
        }

        public Mesh GetMesh(int index) => _lodFilters[index]?.sharedMesh;
    }
}