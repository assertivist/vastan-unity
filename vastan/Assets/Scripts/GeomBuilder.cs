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
        //add_avara_bsp_poly(bsp, bsp.polys[0], marker1, marker2);
        return this;
    }

    delegate void add_verts(List<int> triangle, EdgeRecord e);
    private void add_avara_bsp_poly(AvaraBSP bsp, PolyRecord p, Color marker1, Color marker2)
    {
        //Debug.Log(p1a + " " + p1b);
        //Debug.Log(p2a + " " + p2b);
        //Debug.Log(p3a + " " + p3b);

        //Debug.Log("----");
        var normal_rec = bsp.normal_records[p.normal_index];
        var base_point = normal_rec.base_point_index;
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
        // orderd list of points around this face
        List<int> verts = new List<int>();
        // indexed list of nearest neighbors
        // Dictionary<int, int[]> neighbors = new Dictionary<int, int[]>();

        for (int i = 0; i < p.edge_count; i++)
        {
            EdgeRecord e = bsp.unique_edges[bsp.edges[p.first_edge + i]];
            if(!verts.Contains(e.a)) {
                verts.Add(e.a);
            }
            if(!verts.Contains(e.b)) {
                verts.Add(e.b);
            }
        }

        Vector3 abs_normal = new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z));

        float max_normal_component = Mathf.Max(abs_normal.x, abs_normal.y, abs_normal.z);

        List<Vector2> faceverts = new List<Vector2>();
        
        var start_vert = new_verts.Count;
        foreach(int vidx in verts) {
            var v = bsp.points[vidx];
            if(max_normal_component == abs_normal.x)
                faceverts.Add(new Vector2(v.y, v.z));
            if(max_normal_component == abs_normal.y)
                faceverts.Add(new Vector2(v.x, v.z));
            if(max_normal_component == abs_normal.z)
                faceverts.Add(new Vector2(v.x, v.y));
            new_verts.Add(v);
        }
        var end_vert = new_verts.Count;
        Triangulator tr = new Triangulator(faceverts.ToArray());
        int[] indicies = tr.Triangulate();
        new_triangles.AddRange(indicies.Select(i => i + start_vert));
        add_vert_normals(normal, start_vert, end_vert);
        add_vert_colors(the_color, start_vert, end_vert);

        start_vert = new_verts.Count;
        foreach(int vidx in verts) {
            new_verts.Add(bsp.points[vidx]);
        }
        end_vert = new_verts.Count;
        new_triangles.AddRange(indicies.Reverse().Select(i => i + start_vert));
        add_vert_normals(normal, start_vert, end_vert);
        add_vert_colors(the_color, start_vert, end_vert);



        /*
        for(int i = 2; i < verts.Count; i++) {
            avara_bsp_verts_tri(
                new Vector3[] {
                    bsp.points[verts[0]],
                    bsp.points[verts[i - 1]],
                    bsp.points[verts[i]]
                },
                bsp, normal, UnityEngine.Random.ColorHSV()
            );
        }
         */




        /*
        //Dictionary<int, List<int>> vert_graph = new Dictionary<int, List<int>>();
        add_verts add_edge = (t, e) =>
        {
            if (!t.Contains(e.a)) {
                t.Add(e.a);
            }
            if (!t.Contains(e.b)) {
                t.Add(e.b);
            }
        };
        
        Dictionary<int, int> verts = new Dictionary<int, int>();
        List<int> triangle = new List<int>();
        string tri = "";
        for (int i = 0; i < p.edge_count; i++)
        {
            EdgeRecord e = bsp.unique_edges[bsp.edges[p.first_edge + i]];

            add_edge(triangle, e);
            if (triangle.Count() == 3) {
                var lerp = (float)i / p.edge_count;
                tri = "";
                foreach(int t in triangle) {
                    tri += t + " ";
                    if (verts.ContainsKey(t))
                        verts[t] += 1;
                    else
                        verts.Add(t, 1);
                }
                Debug.Log("Triangle: " + tri);
                avara_bsp_verts_tri(triangle.Take(3).ToList(), bsp, normal, the_color);//Color.Lerp(the_color, Color.white, lerp));
                
                triangle.RemoveRange(0, 3);
            }
        }
        tri = "";
        foreach(int t in triangle) {
            tri += t + " ";
            if (verts.ContainsKey(t))
                verts[t] += 1;
            else
                verts.Add(t, 1);
        }
        Debug.Log("Triangle: " + tri);
        int count = 0;
        foreach(KeyValuePair<int, int> kvp in verts.OrderByDescending(kvp => kvp.Value)) {
            //Debug.Log(kvp.Key + ": " + kvp.Value); 
            if (count < 3) {
                triangle.Add(kvp.Key);
            }
            count++;
        }
        avara_bsp_verts_tri(triangle, bsp, normal, the_color);

         */


        /* 
        foreach(KeyValuePair<int, List<int>> vert_graph_pair in vert_graph) {
            string debug_verts = "";

            foreach(int neighbor in vert_graph_pair.Value) {
                debug_verts += neighbor + " ";
            }
            
            if (vert_graph_pair.Value.Count() > 2) {
                Debug.Log("Tri with more than 3 verts?");
                Debug.Log(vert_graph_pair.Key + ": " + debug_verts);
            }
            Vector3[] vert_arr = {
                bsp.points[vert_graph_pair.Key],
                bsp.points[vert_graph_pair.Value[0]],
                bsp.points[vert_graph_pair.Value[1]]
            };
            avara_bsp_verts_tri(vert_arr, normal, UnityEngine.Random.ColorHSV());

        }
        */
        /* 
        avara_bsp_verts_tri(new Vector3[] {
                bsp.points[vert_graph.First().Key],
                bsp.points[vert_graph.Last().Value[0]],
                bsp.points[vert_graph.Last().Value[1]]
        }, normal, Color.cyan);
        avara_bsp_verts_tri(new Vector3[] {
                bsp.points[vert_graph.Last().Key], 
                bsp.points[vert_graph.First().Value[0]],
                bsp.points[vert_graph.First().Value[1]]
        }, normal, Color.magenta);
        */
    }

    void avara_bsp_verts_tri(Vector3[] verts, AvaraBSP bsp, Vector3 normal, Color c)
    {
        var start_vert = new_verts.Count;
        new_verts.AddRange(verts);
        var end_vert = new_verts.Count;

        new_triangles.Add(start_vert + 2);
        new_triangles.Add(start_vert + 1);
        new_triangles.Add(start_vert);
        
        add_vert_normals(normal, start_vert, end_vert);
        add_vert_colors(c, start_vert, end_vert);
        
        start_vert = new_verts.Count;
        new_verts.AddRange(verts);
        end_vert = new_verts.Count;

        new_triangles.Add(start_vert);
        new_triangles.Add(start_vert + 1);
        new_triangles.Add(start_vert + 2);
        
        add_vert_normals(normal, start_vert, end_vert);
        add_vert_colors(c, start_vert, end_vert);
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
            m.RecalculateNormals();
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
