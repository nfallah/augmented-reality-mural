using UnityEngine;

public class OutlineGenerator
{
    private OutlineGenerator() { }

    /* The mesh triangles are hard-coded because the vertices, regardless of size, position, and-
     * thickness, will always be generated in the same order.
     */
    private static readonly int[] triangles = new int[]
    {
        // Front triangles
        0, 12, 15, 15, 3, 0,
        2, 14, 13, 13, 1, 2,
        4, 12, 13, 13, 5, 4,
        6, 14, 15, 15, 7, 6,

        // Back triangles
        31, 28, 16, 16, 19, 31,
        29, 30, 18, 18, 17, 29,
        29, 28, 20, 20, 21, 29,
        31, 30, 22, 22, 23, 31,

        // Outer triangles
        0, 16, 17, 17, 1, 0,
        2, 18, 19, 19, 3, 2,
        3, 19, 16, 16, 0, 3,
        1, 17, 18, 18, 2, 1,

        // Inner triangles
        21, 20, 4, 4, 5, 21,
        23, 22, 6, 6, 7, 23,
        20, 23, 7, 7, 4, 20,
        22, 21, 5, 5, 6, 22
    };

    public static GameObject GenerateOutline(Vector3 size, Vector3? nullablePos, float thickness)
    {
        // If no position is provided, the world origin is utilized.
        Vector3 pos = (nullablePos == null) ? Vector3.zero : nullablePos.Value;
        Vector3 halfSize = size / 2f;

        GameObject outlineObj = new GameObject("Outline");
        outlineObj.transform.position = pos;
        MeshFilter meshFilter = outlineObj.AddComponent<MeshFilter>();
        outlineObj.AddComponent<MeshRenderer>();

        Vector3[] vertices = new Vector3[32];

        // Outermost vertices, front
        vertices[0] = new Vector3(pos.x - halfSize.x - thickness, pos.y - halfSize.y - thickness, pos.z - thickness);
        vertices[1] = new Vector3(pos.x - halfSize.x - thickness, pos.y + halfSize.y + thickness, pos.z - thickness);
        vertices[2] = new Vector3(pos.x + halfSize.x + thickness, pos.y + halfSize.y + thickness, pos.z - thickness);
        vertices[3] = new Vector3(pos.x + halfSize.x + thickness, pos.y - halfSize.y - thickness, pos.z - thickness);

        // Innermost vertices, front
        vertices[4] = new Vector3(pos.x - halfSize.x, pos.y - halfSize.y, pos.z - thickness);
        vertices[5] = new Vector3(pos.x - halfSize.x, pos.y + halfSize.y, pos.z - thickness);
        vertices[6] = new Vector3(pos.x + halfSize.x, pos.y + halfSize.y, pos.z - thickness);
        vertices[7] = new Vector3(pos.x + halfSize.x, pos.y - halfSize.y, pos.z - thickness);

        // X-reduced vertices, front
        vertices[8] = new Vector3(pos.x - halfSize.x, pos.y - halfSize.y - thickness, pos.z - thickness);
        vertices[9] = new Vector3(pos.x - halfSize.x, pos.y + halfSize.y + thickness, pos.z - thickness);
        vertices[10] = new Vector3(pos.x + halfSize.x, pos.y + halfSize.y + thickness, pos.z - thickness);
        vertices[11] = new Vector3(pos.x + halfSize.x, pos.y - halfSize.y - thickness, pos.z - thickness);

        // Y-reduced vertices, front
        vertices[12] = new Vector3(pos.x - halfSize.x - thickness, pos.y - halfSize.y, pos.z - thickness);
        vertices[13] = new Vector3(pos.x - halfSize.x - thickness, pos.y + halfSize.y, pos.z - thickness);
        vertices[14] = new Vector3(pos.x + halfSize.x + thickness, pos.y + halfSize.y, pos.z - thickness);
        vertices[15] = new Vector3(pos.x + halfSize.x + thickness, pos.y - halfSize.y, pos.z - thickness);

        // Outermost vertices, back
        vertices[16] = new Vector3(pos.x - halfSize.x - thickness, pos.y - halfSize.y - thickness, pos.z + thickness);
        vertices[17] = new Vector3(pos.x - halfSize.x - thickness, pos.y + halfSize.y + thickness, pos.z + thickness);
        vertices[18] = new Vector3(pos.x + halfSize.x + thickness, pos.y + halfSize.y + thickness, pos.z + thickness);
        vertices[19] = new Vector3(pos.x + halfSize.x + thickness, pos.y - halfSize.y - thickness, pos.z + thickness);

        // Innermost vertices, back
        vertices[20] = new Vector3(pos.x - halfSize.x, pos.y - halfSize.y, pos.z + thickness);
        vertices[21] = new Vector3(pos.x - halfSize.x, pos.y + halfSize.y, pos.z + thickness);
        vertices[22] = new Vector3(pos.x + halfSize.x, pos.y + halfSize.y, pos.z + thickness);
        vertices[23] = new Vector3(pos.x + halfSize.x, pos.y - halfSize.y, pos.z + thickness);

        // X-reduced vertices, back
        vertices[24] = new Vector3(pos.x - halfSize.x, pos.y - halfSize.y - thickness, pos.z + thickness);
        vertices[25] = new Vector3(pos.x - halfSize.x, pos.y + halfSize.y + thickness, pos.z + thickness);
        vertices[26] = new Vector3(pos.x + halfSize.x, pos.y + halfSize.y + thickness, pos.z + thickness);
        vertices[27] = new Vector3(pos.x + halfSize.x, pos.y - halfSize.y - thickness, pos.z + thickness);

        // Y-reduced vertices, back
        vertices[28] = new Vector3(pos.x - halfSize.x - thickness, pos.y - halfSize.y, pos.z + thickness);
        vertices[29] = new Vector3(pos.x - halfSize.x - thickness, pos.y + halfSize.y, pos.z + thickness);
        vertices[30] = new Vector3(pos.x + halfSize.x + thickness, pos.y + halfSize.y, pos.z + thickness);
        vertices[31] = new Vector3(pos.x + halfSize.x + thickness, pos.y - halfSize.y, pos.z + thickness);

        meshFilter.mesh = new Mesh()
        {
            vertices = vertices,
            triangles = triangles
        };

        return outlineObj;
    }
}