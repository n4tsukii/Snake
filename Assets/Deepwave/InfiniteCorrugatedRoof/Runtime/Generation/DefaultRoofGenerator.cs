using Deepwave.ICR.Core;
using Deepwave.ICR.Data;
using Deepwave.ICR.Utilities;
using UnityEngine;

namespace Deepwave.ICR.Generation
{
    internal class DefaultRoofGenerator : IRoofGenerator
    {
        public void Generate(Mesh mesh, int lodIndex, BuildContext context)
        {
            var lodProfile = LodUtility.GetProfile(context.Settings, lodIndex);
            MeshBuilder.Build(
                mesh,
                context.Settings,
                context.Settings.WaveProfile,
                lodProfile,
                in context.SplineCache
            );
        }
    }
}