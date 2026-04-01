using System.Collections.Generic;
using UnityEngine;

namespace Deepwave.ICR.Generation
{
    internal static class GeometryModifier
    {
        public static void AddThickness(
            List<Vector3> verts,
            List<Vector2> uvs,
            List<Color> colors,
            List<int> tris,
            float thickness)
        {
            if (thickness <= 0f) return;

            int frontVertCount = verts.Count;
            int frontTriCount = tris.Count;
            float halfThickness = thickness * 0.5f; // Mở rộng mỗi bên 50%

            // Allocation: Array cố định nhanh hơn List khi đã biết kích thước
            Vector3[] calculatedNormals = new Vector3[frontVertCount];

            // 1. Tính toán Weighted Normals (dựa trên diện tích tam giác)
            for (int i = 0; i < frontTriCount; i += 3)
            {
                int i1 = tris[i];
                int i2 = tris[i + 1];
                int i3 = tris[i + 2];

                Vector3 v1 = verts[i1];
                Vector3 v2 = verts[i2];
                Vector3 v3 = verts[i3];

                // Cross product tỉ lệ thuận với diện tích -> normal tự động có trọng số
                Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1);

                calculatedNormals[i1] += normal;
                calculatedNormals[i2] += normal;
                calculatedNormals[i3] += normal;
            }

            // 2. Cập nhật đỉnh mặt trước & Tạo đỉnh mặt sau
            for (int i = 0; i < frontVertCount; i++)
            {
                Vector3 normal = calculatedNormals[i].normalized;
                Vector3 originalPos = verts[i]; // Cache vị trí gốc

                // Dời đỉnh hiện tại ra phía trước (Front Face)
                verts[i] = originalPos + (normal * halfThickness);

                // Tạo đỉnh mới lùi về phía sau (Back Face)
                verts.Add(originalPos - (normal * halfThickness));

                uvs.Add(uvs[i]);
                colors.Add(colors[i]);
            }

            // 3. Tạo tam giác mặt sau (Đảo chiều winding order)
            // Offset bắt đầu từ vị trí sau các đỉnh gốc
            int backOffset = frontVertCount;

            for (int i = 0; i < frontTriCount; i += 3)
            {
                // Logic cũ: giữ nguyên vertex index nhưng cộng thêm offset
                tris.Add(tris[i] + backOffset);
                tris.Add(tris[i + 2] + backOffset); // Swap 1 & 2 để lật mặt
                tris.Add(tris[i + 1] + backOffset);
            }
        }
    }
}