using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;

public struct NormalRecord {
    public int normal_index;
    public int base_point_index;
    public int color_index;
    public int visibility_flags;
}

public struct EdgeRecord {
    public int a;
    public int b;
}

public struct PolyRecord {
    public int first_edge;
    public int edge_count;
    public int normal_index;
    public int front_poly;
    public int back_poly;
    public int visibility;
    public int reserved;
}

public struct ColorRecord {
    public long color;
    public int[] color_cache;
}

public class AvaraBSP {
    public string name;
    public int resid;
    public Vector4 enclosure_point;
    public float enclosure_radius;
    public Vector4 min_bounds;
    public Vector4 max_bounds;


    public List<Vector4> points = new List<Vector4>();
    public List<NormalRecord> normal_records = new List<NormalRecord>();
    public List<int> edges = new List<int>();
    public List<EdgeRecord> unique_edges = new List<EdgeRecord>();
    public List<PolyRecord> polys = new List<PolyRecord>();
    public List<Vector4> vectors = new List<Vector4>();
    public List<ColorRecord> colors = new List<ColorRecord>();

    private Vector4 ArrayToVector4(JSONNode thing) {
        var array = thing.AsArray;
        return new Vector4(
            array[0].AsFloat, 
            array[1].AsFloat, 
            array[2].AsFloat, 
            array[3].AsFloat);
    }

    public AvaraBSP(string json) {
        JSONNode o = JSON.Parse(json);
        resid = o["res_id"].AsInt;
        name = o["name"].Value;
        enclosure_point = ArrayToVector4(o["enclosure_point"]);
        enclosure_radius = o["enclosure_radius"].AsFloat;
        min_bounds = ArrayToVector4(o["min_bounds"]);
        max_bounds = ArrayToVector4(o["max_bounds"]);
        foreach(JSONNode child in o["points"].Children) {
            Vector4 point = ArrayToVector4(child);
            points.Add(point);
        }
        foreach(JSONNode child in o["normals"]) {
            JSONArray arr = child.AsArray;
            NormalRecord nr = new NormalRecord();
            nr.normal_index = arr[0].AsInt;
            nr.base_point_index = arr[1].AsInt;
            nr.color_index = arr[2].AsInt;
            nr.visibility_flags = arr[3].AsInt;
            normal_records.Add(nr);
        }
        foreach(JSONNode child in o["edges"]) {
            edges.Add(child.AsInt);
        }
        foreach(JSONNode child in o["unique_edges"]) {
            JSONArray arr = child.AsArray;
            EdgeRecord er = new EdgeRecord();
            er.a = arr[0].AsInt;
            er.b = arr[1].AsInt;
            unique_edges.Add(er);
        }
        foreach(JSONNode child in o["polys"]) {
            JSONArray arr = child.AsArray;
            PolyRecord pr = new PolyRecord();
            pr.first_edge = arr[0].AsInt;
            pr.edge_count = arr[1].AsInt;
            pr.normal_index = arr[2].AsInt;
            pr.front_poly = arr[3].AsInt;
            pr.back_poly = arr[4].AsInt;
            pr.visibility = arr[5].AsInt;
            pr.reserved = arr[6].AsInt;
            polys.Add(pr);
        }
        foreach(JSONNode child in o["colors"]) {
            JSONArray arr = child.AsArray;
            ColorRecord c = new ColorRecord();
            c.color = arr[0].AsInt;
            //c.color_cache = arr[1].AsArray;
            colors.Add(c);
        }
        foreach(JSONNode child in o["vectors"]) {
            Vector4 v = ArrayToVector4(child);
            vectors.Add(v);
        }
    }
}
