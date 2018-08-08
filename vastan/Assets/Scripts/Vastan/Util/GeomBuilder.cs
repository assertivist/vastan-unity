using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Vastan.Util 
{
	public class GeomBuilder
	{
		private Mesh mesh;
		private List<Vector3> newVerts;
		private List<int> newTriangles;
		private List<Color> newColors;
		private List<Vector3> newNormals;

		public GeomBuilder()
		{
			Init();
		}

		public void Init() 
		{
            mesh = new Mesh();
            mesh.name = System.Guid.NewGuid().ToString();
            newVerts = new List<Vector3>();
			newTriangles = new List<int>();
			newColors = new List<Color>();
            newNormals = new List<Vector3>();
			
		}

		private void AddLastVertsAsQuad()
		{
			int len = newVerts.Count;
			newTriangles.Add(len - 4);
			newTriangles.Add(len - 3);
			newTriangles.Add(len - 2);

			newTriangles.Add(len - 1);
			newTriangles.Add(len - 4);
			newTriangles.Add(len - 2);
		}

		private void AddLastVertsAsTri() 
		{
			int len = newVerts.Count;
			newTriangles.Add(len - 3);
			newTriangles.Add(len - 2);
			newTriangles.Add(len - 1);
		}
        
		private static void AddRepeatingAttribute<T>(List<T> list, T thing, int times)
		{
			list.AddRange(Enumerable.Repeat<T>(thing, times));
		}

		private delegate void VertFromPoints(float x, float y, float z);

		private VertFromPoints VFPPosRot(Vector3 shift, Quaternion rot) {
			return delegate (float x, float y, float z)
			{
				newVerts.Add((rot * new Vector3(x, y, z)) + shift);
			};
		}

		private delegate void VertFromVector(Vector3 vector);

		private VertFromVector VFVPosRot(Vector3 shift, Quaternion rot)
		{
			return delegate (Vector3 vector)
			{
				newVerts.Add(rot * vector + shift);
			};
		}

		public GeomBuilder AddBlock(Color color, 
		                            Vector3 center, 
		                            Vector3 size, 
		                            Quaternion rot) 
		{
			float xShift = size.x / 2.0f;
			float yShift = size.y / 2.0f;
			float zShift = size.z / 2.0f;
			VertFromPoints AddVert = VFPPosRot(center, rot);

			// +x face
            AddVert(+xShift, -yShift, -zShift);
            AddVert(+xShift, +yShift, -zShift);
            AddVert(+xShift, +yShift, +zShift);
            AddVert(+xShift, -yShift, +zShift);
            AddLastVertsAsQuad();

            // -x face
            AddVert(-xShift, +yShift, -zShift);
            AddVert(-xShift, -yShift, -zShift);
            AddVert(-xShift, -yShift, +zShift);
            AddVert(-xShift, +yShift, +zShift);
            AddLastVertsAsQuad();

            // +y face
            AddVert(-xShift, +yShift, +zShift);
            AddVert(+xShift, +yShift, +zShift);
            AddVert(+xShift, +yShift, -zShift);
            AddVert(-xShift, +yShift, -zShift);
            AddLastVertsAsQuad();

            // -y face
            AddVert(-xShift, -yShift, -zShift);
            AddVert(+xShift, -yShift, -zShift);
            AddVert(+xShift, -yShift, +zShift);
            AddVert(-xShift, -yShift, +zShift);
            AddLastVertsAsQuad();

            // +z face
            AddVert(-xShift, +yShift, +zShift);
            AddVert(-xShift, -yShift, +zShift);
            AddVert(+xShift, -yShift, +zShift);
            AddVert(+xShift, +yShift, +zShift);
            AddLastVertsAsQuad();

            // -z face
            AddVert(+xShift, +yShift, -zShift);
            AddVert(+xShift, -yShift, -zShift);
            AddVert(-xShift, -yShift, -zShift);
            AddVert(-xShift, +yShift, -zShift);
            AddLastVertsAsQuad();

			AddRepeatingAttribute<Color>(newColors, color, 24);

			return this;
		}

        public GeomBuilder AddRamp(Color color, 
		                           Vector3 rampBase,
		                           Vector3 rampTop,
		                           float width,
		                           float thiccness,
		                           Quaternion rot) 
		{
			int startVerts = newVerts.Count;
			Vector3 midpoint = (rampTop + rampBase) / 2.0f;

            if (midpoint != Vector3.zero)
			{
				rampBase = (rampBase - (midpoint - Vector3.zero));
				rampTop = (rampTop - (midpoint - Vector3.zero));
			}

			Vector3 p3 = new Vector3(rampTop.x, rampTop.y - thiccness, rampTop.z);
			Vector3 p4 = new Vector3(rampBase.x, rampBase.y - thiccness, rampBase.z);

			Vector3 offset = Vector3.Cross(
				((rampTop + new Vector3(0, -1000, 0)) - rampBase),
				(rampTop - rampBase)).normalized;
			offset *= (width / 2.0f);

			VertFromVector AddVert = VFVPosRot(midpoint, rot);

			if (width != 0 && (p3 - rampBase).magnitude != 0) 
			{
				AddVert(rampTop - offset);
				AddVert(rampBase - offset);
				AddVert(rampBase + offset);
				AddVert(rampTop + offset);
				AddLastVertsAsQuad();

				AddVert(p4 + offset);
				AddVert(p4 - offset);
				AddVert(p3 - offset);
				AddVert(p3 + offset);
				AddLastVertsAsQuad();
			}

			if (width != 0 && thiccness != 0) 
			{
				AddVert(rampTop - offset);
				AddVert(rampTop + offset);
				AddVert(p3 + offset);
				AddVert(p3 - offset);
				AddLastVertsAsQuad();
			}
			if (thiccness !=0 && (p3 - rampBase).magnitude != 0)
			{
				AddVert(rampTop - offset);
				AddVert(p3 - offset);
				AddVert(p4 - offset);
				AddVert(rampBase - offset);
				AddLastVertsAsQuad();

				AddVert(p4 + offset);
				AddVert(p3 + offset);
				AddVert(rampTop + offset);
				AddVert(rampBase + offset);
				AddLastVertsAsQuad();
			}
			int endVerts = newVerts.Count;
			AddRepeatingAttribute<Color>(newColors, color, endVerts - startVerts);
			return this;
		}
	}	



	public static class MiscExtensions
	{
		public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int n)
        {
            return source.Skip(Math.Max(0, source.Count() - n));
        }
	}
}