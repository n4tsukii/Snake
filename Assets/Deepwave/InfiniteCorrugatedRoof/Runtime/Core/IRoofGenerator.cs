using Deepwave.ICR.Data;
using UnityEngine;

namespace Deepwave.ICR.Core
{
    internal interface IRoofGenerator
    {
        void Generate(Mesh mesh, int lodIndex, BuildContext context);
    }
}