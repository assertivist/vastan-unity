using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GeomBuilder {

    private Mesh m;
    private List<Vector3> new_verts;
    private List<int> new_triangles;

    public void init ()
    {
        m = new Mesh();
        new_verts = new List<Vector3>();
        new_triangles = new List<int>();
    }

    public void add_rect (Color c, Vector3 p1, Vector3 p2)
    {
        bool swap_y = p1.x != p2.y;

        new_verts.Add(p1);

        if (swap_y)
            new_verts.Add(new Vector3(p2.x, p1.y, p1.z));
        else
            new_verts.Add(new Vector3(p2.x, p2.y, p1.z));

        new_verts.Add(p2);

        if (swap_y)
            new_verts.Add(new Vector3(p1.x, p2.y, p2.z));
        else
            new_verts.Add(new Vector3(p1.x, p1.y, p2.z));


        int len = new_verts.Count;
        new_triangles.Add(len - 4);
        new_triangles.Add(len - 3);
        new_triangles.Add(len - 2);

        new_triangles.Add(len - 2);
        new_triangles.Add(len - 3);
        new_triangles.Add(len - 4);
    }

    public Mesh build_mesh ()
    {
        m = new Mesh();
        m.vertices = new_verts.ToArray();
        m.triangles = new_triangles.ToArray();
        return m;
    }
	
}
