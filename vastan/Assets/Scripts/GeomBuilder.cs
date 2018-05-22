using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

// so, this is pretty much a direct port of
// geombuilder from projects past. i totally
// forgot that LINQ existed until i was 
// already mostly finished with this. 

// also
// note that there is no vertex sharing in 
// the shape definitions. This is because
// UNITU will attempt to smooth faces that 
// share vertices (a cube becomes shaded like
// a sphere) unless you duplicate faces/verts.
// (normals are set per vertex, not per face)

// ALSO 
// UNITU is Y-UP

public class GeomBuilder {

    private Mesh m;
    private List<Vector3> new_verts;
    private List<int> new_triangles;
    private List<Color> new_colors;
    private List<Vector3> new_normals;
    private delegate void AddVertXYZ(float p1, 
                                     float p2, 
                                     float p3);
    private delegate void AddVertV3(Vector3 v);

    public void init() {
        m = new Mesh();
        m.name = uuid();
        new_verts = new List<Vector3>();
        new_triangles = new List<int>();
        new_colors = new List<Color>();
        new_normals = new List<Vector3>();
    }

    private void add_last_verts_as_quad () {
        int len = new_verts.Count;
        new_triangles.Add(len - 4);
        new_triangles.Add(len - 3);
        new_triangles.Add(len - 2);

        new_triangles.Add(len - 1);
        new_triangles.Add(len - 4);
        new_triangles.Add(len - 2);
    }

    private void add_last_verts_as_tri () {
        int len = new_verts.Count;

        new_triangles.Add(len - 3);
        new_triangles.Add(len - 2);
        new_triangles.Add(len - 1);
    }
    
    private void add_vert_colors(Color c, 
                                 int start_verts, 
                                 int end_verts) {
        int i = 0;
        while (i < (end_verts - start_verts)) {
            new_colors.Add(c);
            i++;
        }
    }

    private void add_vert_normals(Vector3 normal,
                                  int start_verts,
                                  int end_verts) {
        int i = 0;
        while (i < (end_verts - start_verts)) {
            new_normals.Add(normal);
            i++;
        }
    }

    public GeomBuilder add_block(Color c, 
                                 Vector3 center, 
                                 Vector3 size, 
                                 Quaternion rot) {
        float x_shift = size.x / 2.0f;
        float y_shift = size.y / 2.0f;
        float z_shift = size.z / 2.0f;

        AddVertXYZ add_vert = delegate(float p1, 
                                       float p2, 
                                       float p3) {
            new_verts.Add((rot * 
                new Vector3(p1, p2, p3)) + center);
        };

        // +x face
        add_vert(+x_shift, -y_shift, -z_shift);
        add_vert(+x_shift, +y_shift, -z_shift);
        add_vert(+x_shift, +y_shift, +z_shift);
        add_vert(+x_shift, -y_shift, +z_shift);
        add_last_verts_as_quad();

        // -x face
        add_vert(-x_shift, +y_shift, -z_shift);
        add_vert(-x_shift, -y_shift, -z_shift);
        add_vert(-x_shift, -y_shift, +z_shift);
        add_vert(-x_shift, +y_shift, +z_shift);
        add_last_verts_as_quad();

        // +y face
        add_vert(-x_shift, +y_shift, +z_shift);
        add_vert(+x_shift, +y_shift, +z_shift);
        add_vert(+x_shift, +y_shift, -z_shift);
        add_vert(-x_shift, +y_shift, -z_shift);
        add_last_verts_as_quad();

        // -y face
        add_vert(-x_shift, -y_shift, -z_shift);
        add_vert(+x_shift, -y_shift, -z_shift);
        add_vert(+x_shift, -y_shift, +z_shift);
        add_vert(-x_shift, -y_shift, +z_shift);
        add_last_verts_as_quad();

        // +z face
        add_vert(-x_shift, +y_shift, +z_shift);
        add_vert(-x_shift, -y_shift, +z_shift);
        add_vert(+x_shift, -y_shift, +z_shift);
        add_vert(+x_shift, +y_shift, +z_shift);
        add_last_verts_as_quad();

        // -z face
        add_vert(+x_shift, +y_shift, -z_shift);
        add_vert(+x_shift, -y_shift, -z_shift);
        add_vert(-x_shift, -y_shift, -z_shift);
        add_vert(-x_shift, +y_shift, -z_shift);
        add_last_verts_as_quad();
        
        int i = 0;
        while(i < 24) {
            new_colors.Add(c);
            i++;
        }
        return this; 
    }

    public GeomBuilder add_ramp(Color c, 
                         Vector3 ramp_base, 
                         Vector3 ramp_top, 
                         float width, 
                         float thickness, 
                         Quaternion rot) {
        int start_verts = new_verts.Count;
        Vector3 midpoint = (ramp_top + ramp_base) / 2.0f;

        if (midpoint != Vector3.zero) {
            ramp_base = (ramp_base - (midpoint - Vector3.zero));
            ramp_top = (ramp_top - (midpoint - Vector3.zero));
        }

        Vector3 p3 = new Vector3(ramp_top.x, 
            ramp_top.y - thickness, ramp_top.z);
        Vector3 p4 = new Vector3(ramp_base.x, 
            ramp_base.y - thickness, ramp_base.z);

        Vector3 offset = Vector3.Cross(
            ((ramp_top + new Vector3(0, -1000, 0)) - ramp_base), 
            (ramp_top - ramp_base)).normalized;
        offset *= (width / 2.0f);

        AddVertV3 add_vert = delegate (Vector3 v) {
            new_verts.Add(rot * v + midpoint);
        };

        if (width != 0 && (p3 - ramp_base).magnitude != 0) {
            add_vert(ramp_top - offset);
            add_vert(ramp_base - offset);
            add_vert(ramp_base + offset);
            add_vert(ramp_top + offset);
            add_last_verts_as_quad();

            add_vert(p4 + offset);
            add_vert(p4 - offset);
            add_vert(p3 - offset);
            add_vert(p3 + offset);
            add_last_verts_as_quad();
        }
        if (width != 0 && thickness != 0) {
            add_vert(ramp_top - offset);
            add_vert(ramp_top + offset);
            add_vert(p3 + offset);
            add_vert(p3 - offset);
            add_last_verts_as_quad();

            add_vert(p4 - offset);
            add_vert(p4 + offset);
            add_vert(ramp_base + offset);
            add_vert(ramp_base - offset);
            add_last_verts_as_quad();
        }
        if (thickness != 0 && (p3 - ramp_base).magnitude != 0) {
            add_vert(ramp_top - offset);
            add_vert(p3 - offset);
            add_vert(p4 - offset);
            add_vert(ramp_base - offset);
            add_last_verts_as_quad();

            add_vert(p4 + offset);
            add_vert(p3 + offset);
            add_vert(ramp_top + offset);
            add_vert(ramp_base + offset);
            add_last_verts_as_quad();
        }

        int end_verts = new_verts.Count;
        add_vert_colors(c, start_verts, end_verts);
        return this;
    }

    public GeomBuilder add_wedge(Color c, 
                                 Vector3 wedge_base,
                                 Vector3 wedge_top,
                                 float width,
                                 Quaternion rot) {
        int start_verts = new_verts.Count;
        float delta_y = wedge_top.y - wedge_base.y;
        Vector3 midpoint = (wedge_top + wedge_base) / 2.0f;
        if (midpoint != Vector3.zero) {
            wedge_base = (wedge_base - (midpoint - Vector3.zero));
            wedge_top = (wedge_top - (midpoint - Vector3.zero));
        }
        Vector3 p3 = new Vector3(wedge_top.x, wedge_base.y, wedge_top.z);

        Vector3 direction;
        if (wedge_base.y > wedge_top.y) {
            direction = new Vector3(0, 1000, 0);
        }
        else {
            direction = new Vector3(0, -1000, 0);
        }
        Vector3 offset = Vector3.Cross(
            ((wedge_top + direction) - wedge_base), 
            (wedge_top - wedge_base)).normalized;
        offset *= (width / 2.0f);

        AddVertV3 add_vert = delegate (Vector3 v) {
            new_verts.Add(rot * v + midpoint);
        };

        if(width != 0 || delta_y != 0) {
            add_vert(wedge_top - offset);
            add_vert(wedge_base - offset);
            add_vert(wedge_base + offset);
            add_vert(wedge_top + offset);
            add_last_verts_as_quad();
        }
        if(width != 0 && (p3 - wedge_base).magnitude != 0) {
            add_vert(p3 - offset);
            add_vert(p3 + offset);
            add_vert(wedge_base + offset);
            add_vert(wedge_base - offset);
            add_last_verts_as_quad();
        }
        if(width != 0 && delta_y != 0.0) {
            add_vert(wedge_top - offset);
            add_vert(wedge_top + offset);
            add_vert(p3 + offset);
            add_vert(p3 - offset);
            add_last_verts_as_quad();
        }
        if (delta_y != 0 && (p3 - wedge_base).magnitude != 0) {
            add_vert(p3 - offset);
            add_vert(wedge_base - offset);
            add_vert(wedge_top - offset);
            add_last_verts_as_tri();

            add_vert(wedge_top + offset); 
            add_vert(wedge_base + offset);
            add_vert(p3 + offset);
            add_last_verts_as_tri();
        }
        int end_verts = new_verts.Count;
        add_vert_colors(c, start_verts, end_verts);
        return this;
    }

    public GeomBuilder add_dome (Color c,
                                 Vector3 center,
                                 float radius,
                                 int samples,
                                 int planes,
                                 Quaternion rot) {
        int start_verts = new_verts.Count;
        float two_pi = Mathf.PI * 2;
        float half_pi = Mathf.PI / 2;
        var azimuths = (from x in Enumerable.Range(0, samples + 1) 
            select (two_pi * x) / samples).ToList<float>();
        var elevations = (from x in Enumerable.Range(0, planes) 
            select (half_pi * x) / (planes - 1)).ToList<float>();
        AddVertV3 add_vert = delegate(Vector3 v) {
            new_verts.Add(rot * v + center);
        };
        foreach (int i in Enumerable.Range(0, elevations.Count() - 2)) {
            foreach(int j in Enumerable.Range(0, azimuths.Count() - 1)) {
                Vector3 p1 = to_cartesian(azimuths.ElementAt(j), 
                    elevations.ElementAt(i), radius);

                Vector3 p2 = to_cartesian(azimuths.ElementAt(j), 
                    elevations.ElementAt(i + 1), radius);

                Vector3 p3 = to_cartesian(azimuths.ElementAt(j + 1), 
                    elevations.ElementAt(i + 1), radius);

                Vector3 p4 = to_cartesian(azimuths.ElementAt(j + 1), 
                    elevations.ElementAt(i), radius);

                add_vert(p1);
                add_vert(p2);
                add_vert(p3);
                add_vert(p4);
                add_last_verts_as_quad();
            }
        }
        foreach (int k in Enumerable.Range(0, azimuths.Count() - 1)) {
            Vector3 p1 = to_cartesian(azimuths.ElementAt(k), 
                elevations.ElementAt(elevations.Count() - 2), radius);

            Vector3 p2 = new Vector3(0, radius, 0);

            Vector3 p3 = to_cartesian(azimuths.ElementAt(k + 1), 
                elevations.ElementAt(elevations.Count() - 2), radius);

            add_vert(p1);
            add_vert(p2);
            add_vert(p3);
            add_last_verts_as_tri();
        }
        int end_verts = new_verts.Count;
        add_vert_colors(c, start_verts, end_verts);
        return this;                          
    }

    public GeomBuilder add_avara_bsp(string json, Color marker1, Color marker2) {
        AvaraBSP bsp = new AvaraBSP(json);
        Debug.Log("Polys: " + bsp.polys.Count);
        Debug.Log("Unique Edges: " + bsp.unique_edges.Count);
        Debug.Log("Edges: " + bsp.edges.Count);
        Debug.Log("Points: " + bsp.points.Count);
        //foreach (Vector4 point in bsp.points) {
        //    new_verts.Add(point);
        //}

        //PolyRecord p = bsp.polys[0];

        foreach (PolyRecord p in bsp.polys) {
            add_avara_bsp_poly(bsp, p, marker1, marker2);
        }
        return this;
    }

    private void add_avara_bsp_poly(AvaraBSP bsp, PolyRecord p, Color marker1, Color marker2)
    {
        //Debug.Log(p1a + " " + p1b);
        //Debug.Log(p2a + " " + p2b);
        //Debug.Log(p3a + " " + p3b);

        var normal_rec = bsp.normal_records[p.normal_index];
        //Debug.Log(normal_rec.normal_index);
        var normal = bsp.vectors[normal_rec.normal_index];
        var color_rec = bsp.colors[normal_rec.color_index];
        var color_long = (int)color_rec.color;
        //var b = color_long / 32767;
        //var g = (color_long - b * 32767) / 256;
        //var r = color_long - b * 32767 - g * 256;

        int b = (color_long >> 24) & 0xff;
        int g = (color_long >> 16) & 0xff;
        int r = (color_long >> 8) & 0xff;
        int a = color_long & 0xff;

        Color the_color = new Color(r / 254f, g / 254f, b / 254f, a / 254f);

        if ( // marker 1
            the_color.r == 1 &&
            the_color.g == 1 &&
            the_color.b == 0 &&
            the_color.a == 1
            )
        {
            the_color = marker1;
        }

        if ( // marker 2
            the_color.r == 0 &&
            the_color.g == 1 &&
            the_color.b == 0 &&
            the_color.a == 0
           )
        {
            the_color = marker2;
        }

        add_avara_bsp_verts(bsp, p, normal, the_color);
        
    }

    private void add_avara_bsp_verts(AvaraBSP bsp, PolyRecord p, Vector3 normal, Color color)
    {
        List<EdgeRecord> edges = new List<EdgeRecord>();
        for (int i = 0; i < p.edge_count; i++)
        {
            edges.Add(bsp.unique_edges[bsp.edges[p.first_edge + i]]);
        }
        HashSet<int> unique = new HashSet<int>();
        foreach (EdgeRecord e in edges)
        {
            unique.Add(e.a);
            unique.Add(e.b);
        }
        var face_verts = unique.ToArray();

        avara_bsp_verts_tri(face_verts, normal, color, bsp.points, false);
        avara_bsp_verts_tri(face_verts, normal, color, bsp.points, true);

        return;
        /*
        var p1 = bsp.edges[p.first_edge];
        var p2 = bsp.edges[p.first_edge + 1];
        var p3 = bsp.edges[p.first_edge + 2];
        var ue1 = bsp.unique_edges[p1];
        var p1a = ue1.a;
        var p1b = ue1.b;
        var ue2 = bsp.unique_edges[p2];
        var p2a = ue2.a;
        var p2b = ue2.b;
        var ue3 = bsp.unique_edges[p3];
        var p3a = ue3.a;
        var p3b = ue3.b;

        start_vert = new_verts.Count;

        // We get six points of which 3 are unique
        //HashSet<int> unique = new HashSet<int>();
        unique.Add(p1a);
        unique.Add(p1b);
        unique.Add(p2a);
        unique.Add(p2b);
        unique.Add(p3a);
        unique.Add(p3b);
        face_verts = unique.ToArray();

        // Add unique verts
        foreach (int vert in face_verts)
        {
            new_verts.Add(bsp.points[vert]);
        }

        // poly contains one tri
        if (face_verts.Count() == 3) {
            new_triangles.Add(start_vert + 2);
            new_triangles.Add(start_vert + 1);
            new_triangles.Add(start_vert);
        }
        // poly contains two tris (quad)
        else if (face_verts.Count() == 4)
        {
            // first tri
            new_triangles.Add(start_vert + 2);
            new_triangles.Add(start_vert + 1);
            new_triangles.Add(start_vert);
            // second tri
            new_triangles.Add(start_vert + 3);
            new_triangles.Add(start_vert + 2);
            new_triangles.Add(start_vert + 1);
        }
        add_vert_normals(normal, start_vert, end_vert);
        add_vert_colors(color, start_vert, end_vert);


        start_vert = new_verts.Count;
        // Add verts again for back face
        foreach (int vert in face_verts)
        {
            new_verts.Add(bsp.points[vert]);
        }
        // Add back triangle
        // poly contains one tri
        if (face_verts.Count() == 3)
        {
            new_triangles.Add(start_vert);
            new_triangles.Add(start_vert + 1);
            new_triangles.Add(start_vert + 2);
        }
        // poly contains two tris (quad)
        else if (face_verts.Count() == 4)
        {
            // first tri
            new_triangles.Add(start_vert);
            new_triangles.Add(start_vert + 1);
            new_triangles.Add(start_vert + 2);
            // second tri
            new_triangles.Add(start_vert + 1);
            new_triangles.Add(start_vert + 2);
            new_triangles.Add(start_vert + 3);
        }

        end_vert = new_verts.Count;
        add_vert_normals(normal * -1, start_vert, end_vert);
        add_vert_colors(color, start_vert, end_vert);
        
        start_vert = new_verts.Count;
        new_verts.Add(bsp.points[p3b]);
        new_verts.Add(bsp.points[p3a]);
        new_verts.Add(bsp.points[p2b]);
        new_verts.Add(bsp.points[p2a]);
        new_verts.Add(bsp.points[p1b]);
        new_verts.Add(bsp.points[p1a]);
        new_triangles.Add(start_vert);
        new_triangles.Add(start_vert + 1);
        new_triangles.Add(start_vert + 2);
        new_triangles.Add(start_vert + 1);
        new_triangles.Add(start_vert + 2);
        new_triangles.Add(start_vert + 3);
        new_triangles.Add(start_vert + 2);
        new_triangles.Add(start_vert + 3);
        new_triangles.Add(start_vert + 4);
        new_triangles.Add(start_vert + 3);
        new_triangles.Add(start_vert + 4);
        new_triangles.Add(start_vert + 5);
        end_vert = new_verts.Count;
        add_vert_normals(normal * -1, start_vert, end_vert);
        add_vert_colors(color, start_vert, end_vert);*/
    }
    void avara_bsp_verts_tri(int[] face_verts, Vector3 normal, Color c, List<Vector4> points, bool backface)
    {

        var start_vert = new_verts.Count;
        foreach (int vert in face_verts)
        {
            new_verts.Add(points[vert]);
        }
        var end_vert = new_verts.Count;
        if (backface)
            add_vert_normals(normal * -1, start_vert, end_vert);
        else
            add_vert_normals(normal, start_vert, end_vert);
        add_vert_colors(c, start_vert, end_vert);
        var unique_count = face_verts.Count();

        if (backface)
        {
            // poly contains one tri
            if (unique_count == 3)
            {
                new_triangles.Add(start_vert);
                new_triangles.Add(start_vert + 1);
                new_triangles.Add(start_vert + 1);
            }
            // poly contains two tris (quad)
            else if (unique_count == 4)
            {
                // first tri
                new_triangles.Add(start_vert);
                new_triangles.Add(start_vert + 1);
                new_triangles.Add(start_vert + 2);
                // second tri
                new_triangles.Add(start_vert + 1);
                new_triangles.Add(start_vert + 2);
                new_triangles.Add(start_vert + 3);
            }
            else
            {
                Debug.Log("UNSUPPORTED # OF EDGES: " + unique_count);
            }
        }
        else
        {
            // poly contains one tri
            if (unique_count == 3)
            {
                new_triangles.Add(start_vert + 2);
                new_triangles.Add(start_vert + 1);
                new_triangles.Add(start_vert);
            }
            // poly contains two tris (quad)
            else if (unique_count == 4)
            {
                // first tri
                new_triangles.Add(start_vert + 2);
                new_triangles.Add(start_vert + 1);
                new_triangles.Add(start_vert);
                // second tri
                new_triangles.Add(start_vert + 3);
                new_triangles.Add(start_vert + 2);
                new_triangles.Add(start_vert + 1);
            }
            else
            {
                Debug.Log("UNSUPPORTED # OF EDGES: " + unique_count);
            }
        }
    }

    public static Vector3 to_cartesian(float azimuth, 
                                 float elevation, 
                                 float length) {
        float x = length * Mathf.Sin(azimuth) * Mathf.Cos(elevation);
        float y = length * Mathf.Sin(elevation);
        float z = -length * Mathf.Cos(azimuth) * Mathf.Cos(elevation);
        return new Vector3(x, y, z);
    }

    public Mesh get_mesh () {
        m.vertices = new_verts.ToArray();
        m.colors = new_colors.ToArray();
        m.triangles = new_triangles.ToArray();
        if (new_normals.Count > 0) {
            m.normals = new_normals.ToArray();
        }
        else
            m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    // silliness
    private static string uuid() {
        var random = new System.Random();
        DateTime epoch_start = 
            new System.DateTime(1970, 1, 1, 8, 0, 0,
            System.DateTimeKind.Utc);
        double timestamp = 
            (System.DateTime.UtcNow - epoch_start)
            .TotalSeconds;

        return String.Format("{0:X}", 
            Convert.ToInt32(timestamp))
                + "-" + String.Format("{0:X}", 
                Convert.ToInt32(Time.time * 1000000))
                + "-" + String.Format("{0:X}", 
                random.Next(1000000000));
    }
}
