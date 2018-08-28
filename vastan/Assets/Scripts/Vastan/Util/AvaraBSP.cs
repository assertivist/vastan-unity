using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SimpleJSON;


namespace Vastan.Util.BSP
{
	public struct Triangle
	{
		public int a, b, c;
		public Triangle(int ina, int inb, int inc)
		{
			a = ina;
			b = inb;
			c = inc;
		}
	}

	public struct NormalRecord
	{
		public int normalIndex;
		public int basePointIndex;
		public int colorIndex;
		public int visibilityFlags;
	}

	public struct EdgeRecord
	{
		public int a, b;
	}

	public struct PolyRecord
	{
		public int firstEdge;
		public int edgeCount;
		public int normalIndex;
		public int frontPoly;
		public int backPoly;
		public int visibility;
		public int reserved;
	}

	public class AvaraBSP
	{
		public string name;
		public int resID;
		public Vector4 enclosurePoint;
		public float enclosureRadius;
		public Vector4 minBounds;
		public Vector4 maxBounds;


		public List<Vector4> points = new List<Vector4>();
		public List<NormalRecord> normalRecords = new List<NormalRecord>();
		public List<int> edges = new List<int>();
		public List<EdgeRecord> uniqueEdges = new List<EdgeRecord>();
		public List<PolyRecord> polys = new List<PolyRecord>();
		public List<Vector4> vectors = new List<Vector4>();
		public List<Vector4> colors = new List<Vector4>();
		public List<List<Triangle>> triangles = new List<List<Triangle>>();
		public List<List<int>> trianglesVerts = new List<List<int>>();

		private Vector4 ArrayToVector4(JSONNode thing)
		{
			var array = thing.AsArray;
			return new Vector4(
				array[0].AsFloat,
				array[1].AsFloat,
				array[2].AsFloat,
				array[3].AsFloat);
		}

		public AvaraBSP(string json)
		{
			JSONNode o = JSON.Parse(json);
			resID = o["res_id"].AsInt;
			name = o["name"].Value;
			enclosurePoint = ArrayToVector4(o["enclosure_point"]);
			enclosureRadius = o["enclosure_radius"].AsFloat;
			minBounds = ArrayToVector4(o["min_bounds"]);
			maxBounds = ArrayToVector4(o["max_bounds"]);
			foreach (JSONNode child in o["points"].Children)
			{
				Vector4 point = ArrayToVector4(child);
				points.Add(point);
			}
			foreach (JSONNode child in o["normals"])
			{
				JSONArray arr = child.AsArray;
				NormalRecord nr = new NormalRecord();
				nr.normalIndex = arr[0].AsInt;
				nr.basePointIndex = arr[1].AsInt;
				nr.colorIndex = arr[2].AsInt;
				nr.visibilityFlags = arr[3].AsInt;
				normalRecords.Add(nr);
			}
			foreach (JSONNode child in o["edges"])
			{
				edges.Add(child.AsInt);
			}
			foreach (JSONNode child in o["unique_edges"])
			{
				JSONArray arr = child.AsArray;
				EdgeRecord er = new EdgeRecord();
				er.a = arr[0].AsInt;
				er.b = arr[1].AsInt;
				uniqueEdges.Add(er);
			}
			foreach (JSONNode child in o["polys"])
			{
				JSONArray arr = child.AsArray;
				PolyRecord pr = new PolyRecord();
				pr.firstEdge = arr[0].AsInt;
				pr.edgeCount = arr[1].AsInt;
				pr.normalIndex = arr[2].AsInt;
				pr.frontPoly = arr[3].AsInt;
				pr.backPoly = arr[4].AsInt;
				pr.visibility = arr[5].AsInt;
				pr.reserved = arr[6].AsInt;
				polys.Add(pr);
			}
			foreach (JSONNode child in o["colors"])
			{
				JSONArray arr = child.AsArray;
				colors.Add(ArrayToVector4(arr));
			}
			foreach (JSONNode child in o["vectors"])
			{
				Vector4 v = ArrayToVector4(child);
				vectors.Add(v);
			}

			foreach (JSONNode child in o["triangles_poly"])
			{
				JSONArray arr = child.AsArray;
				triangles.Add(
					arr.Linq.Select(i =>
						new Triangle(
							i.Value.Children.ElementAt(0).AsInt,
							i.Value.Children.ElementAt(1).AsInt,
							i.Value.Children.ElementAt(2).AsInt)).ToList());
			}
			foreach (JSONNode child in o["triangles_verts_poly"])
			{
				trianglesVerts.Add(child.Children.Select(j => j.AsInt).ToList());
			}
		}
	}
}