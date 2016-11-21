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

    private Vector3 to_cartesian(float azimuth, 
                                 float elevation, 
                                 float length) {
        float x = length * Mathf.Sin(azimuth) * Mathf.Cos(elevation);
        float y = length * Mathf.Sin(elevation);
        float z = -length * Mathf.Cos(azimuth) * Mathf.Cos(elevation);
        return new Vector3(x, y, z);
    }

    public Mesh get_mesh () {
        m = new Mesh();
        m.name = uuid();

        m.vertices = new_verts.ToArray();
        m.colors = new_colors.ToArray();
        m.triangles = new_triangles.ToArray();

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
