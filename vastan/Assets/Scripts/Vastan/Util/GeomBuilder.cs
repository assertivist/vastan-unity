using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Vastan.Util.BSP;

namespace Vastan.Util 
{
	public class GeomBuilder
	{
		/// <summary>
        /// Utility class for creating Meshes for unity to
		/// consume. We dynamically create a Mesh by exposing
		/// an API for adding primitive shapes. These functions
		/// collect points, triangles, colors, and optionally 
		/// normals based on the input. These functions can be 
		/// chained, for example:
		///     Mesh twoBlocks = new GeomBuilder().AddBlock(...).AddBlock(..);
        /// </summary>
  
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
      
        /// <summary>
		/// Adds last verts added as two triangles.
        /// </summary>
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

		/// <summary>
		/// Adds the last verts added as a triangle.
		/// </summary>
		private void AddLastVertsAsTri()
		{
			int len = newVerts.Count;
			newTriangles.Add(len - 3);
			newTriangles.Add(len - 2);
			newTriangles.Add(len - 1);
		}

        /// <summary>
        /// Adds a repeating attribute to one of the arrays
        /// </summary>
        /// <param name="list">One of our mesh lists</param>
        /// <param name="thing">The object to be repeated</param>
        /// <param name="times">Repeated this many times</param>
        /// <typeparam name="T">Type of object added</typeparam>
		/// <remarks>This is useful for adding colors and normals
		/// which do not change from vertex to vertex on a single
		/// face.</remarks>
		private static void AddRepeatingAttribute<T>(List<T> list,
		                                             T thing, 
		                                             int times)
		{
			list.AddRange(Enumerable.Repeat<T>(thing, times));
		}

		private delegate void VertexFromPoint(float x, float y, float z);
        /// <summary>
        /// This returns a delegate which will add a single point 
		/// with a translation and rotation applied, accepting 
		/// x y z values
        /// </summary>
        /// <returns>A delegate you can call to add a point with
		/// a translation and rotation applied.</returns>
        /// <param name="trans">Translation vector.</param>
        /// <param name="rot">Rotation to apply.</param>
		private VertexFromPoint VFPTransRot(Vector3 trans, Quaternion rot)
		{
			return delegate (float x, float y, float z)
			{
				newVerts.Add((rot * new Vector3(x, y, z)) + trans);
			};
		}
        
		private delegate void VertexFromVector(Vector3 vector);
        /// <summary>
		/// This returns a delegate which will add a single point 
        /// with a translation and rotation applied, accepting 
		/// a Vector3.
        /// </summary>
		/// <returns>A delegate you can call to add a point with
        /// a translation and rotation applied.</returns>
        /// <param name="trans">Trans.</param>
        /// <param name="rot">Rot.</param>
		private VertexFromVector VFVTransRot(Vector3 trans, Quaternion rot)
		{
			return delegate (Vector3 vector)
			{
				newVerts.Add(rot * vector + trans);
			};
		}

        /// <summary>
        /// Helper function to turn celestial coordinates
		/// into a vector we can use.
        /// </summary>
        /// <returns>A position vector</returns>
        /// <param name="azimuth">Azimuth.</param>
        /// <param name="elevation">Elevation.</param>
        /// <param name="length">Length.</param>
        private static Vector3 CelestialToCartesian(float azimuth,
		                                            float elevation,
		                                            float length)
		{
			return new Vector3(
				length * Mathf.Sin(azimuth) * Mathf.Cos(elevation),
				length * Mathf.Sin(elevation),
			    -length * Mathf.Cos(azimuth) * Mathf.Cos(elevation)
			);
		}
        /// <summary>
        /// Adds a block.
        /// </summary>
		/// <returns>GeomBuilder with a new block</returns>
        /// <param name="color">Color.</param>
        /// <param name="center">Center.</param>
        /// <param name="size">Size.</param>
        /// <param name="rot">Rot.</param>
		public GeomBuilder AddBlock(Color color, 
		                            Vector3 center, 
		                            Vector3 size, 
		                            Quaternion rot) 
		{
			float xShift = size.x / 2.0f;
			float yShift = size.y / 2.0f;
			float zShift = size.z / 2.0f;
			VertexFromPoint AddVert = VFPTransRot(center, rot);

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

        /// <summary>
        /// Adds a ramp
        /// </summary>
        /// <returns>GeomBuilder</returns>
        /// <param name="color">Color.</param>
        /// <param name="rampBase">Ramp base.</param>
        /// <param name="rampTop">Ramp top.</param>
        /// <param name="width">Width.</param>
        /// <param name="thiccness">Thiccness.</param>
        /// <param name="rot">Rotation.</param>
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
            
			VertexFromVector AddVert = VFVTransRot(midpoint, rot);

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

        /// <summary>
        /// Adds a wedge
        /// </summary>
        /// <returns>GeomBuilder</returns>
        /// <param name="color">Color.</param>
        /// <param name="wedgeBase">Wedge base.</param>
        /// <param name="wedgeTop">Wedge top.</param>
        /// <param name="width">Width.</param>
        /// <param name="rot">Rotation.</param>
		public GeomBuilder AddWedge(Color color,
		                            Vector3 wedgeBase,
                                    Vector3 wedgeTop,
                                    float width,
		                            Quaternion rot) 
		{
			int startVerts = newVerts.Count;
            float deltaY = wedgeTop.y - wedgeBase.y;
            Vector3 midpoint = (wedgeTop + wedgeBase) / 2.0f;
            if (midpoint != Vector3.zero)
            {
                wedgeBase = (wedgeBase - (midpoint - Vector3.zero));
                wedgeTop = (wedgeTop - (midpoint - Vector3.zero));
            }
            Vector3 p3 = new Vector3(wedgeTop.x, wedgeBase.y, wedgeTop.z);

            Vector3 direction;
            if (wedgeBase.y > wedgeTop.y)
            {
                direction = new Vector3(0, 1000, 0);
            }
            else
            {
                direction = new Vector3(0, -1000, 0);
            }
            Vector3 offset = Vector3.Cross(
                ((wedgeTop + direction) - wedgeBase),
                (wedgeTop - wedgeBase)).normalized;
            offset *= (width / 2.0f);

			VertexFromVector AddVert = VFVTransRot(midpoint, rot);

            if (width != 0 || deltaY != 0)
            {
                AddVert(wedgeTop - offset);
                AddVert(wedgeBase - offset);
                AddVert(wedgeBase + offset);
                AddVert(wedgeTop + offset);
                AddLastVertsAsQuad();
            }
            if (width != 0 && (p3 - wedgeBase).magnitude != 0)
            {
                AddVert(p3 - offset);
                AddVert(p3 + offset);
                AddVert(wedgeBase + offset);
                AddVert(wedgeBase - offset);
                AddLastVertsAsQuad();
            }
            if (width != 0 && deltaY != 0.0)
            {
                AddVert(wedgeTop - offset);
                AddVert(wedgeTop + offset);
                AddVert(p3 + offset);
                AddVert(p3 - offset);
                AddLastVertsAsQuad();
            }
            if (deltaY != 0 && (p3 - wedgeBase).magnitude != 0)
            {
                AddVert(p3 - offset);
                AddVert(wedgeBase - offset);
                AddVert(wedgeTop - offset);
                AddLastVertsAsTri();

                AddVert(wedgeTop + offset);
                AddVert(wedgeBase + offset);
                AddVert(p3 + offset);
                AddLastVertsAsTri();
            }
            int endVerts = newVerts.Count;
			AddRepeatingAttribute<Color>(newColors, color, endVerts - startVerts);
            return this;
		}

        /// <summary>
        /// Adds a dome
        /// </summary>
        /// <returns>GeomBuilder</returns>
        /// <param name="color">Color.</param>
        /// <param name="center">Center.</param>
        /// <param name="radius">Radius.</param>
        /// <param name="samples">Samples.</param>
        /// <param name="planes">Planes.</param>
        /// <param name="rot">Rotation.</param>
        public GeomBuilder AddDome(Color color,
                                     Vector3 center,
                                     float radius,
                                     int samples,
                                     int planes,
                                     Quaternion rot)
        {
			int startVerts = newVerts.Count;
			float twoPi = Mathf.PI * 2;
			float halfPi = Mathf.PI / 2;
            var azimuths = (from x in Enumerable.Range(0, samples + 1)
                            select (twoPi * x) / samples).ToList<float>();
            var elevations = (from x in Enumerable.Range(0, planes)
                              select (halfPi * x) / (planes - 1)).ToList<float>();
            
			VertexFromVector AddVert = VFVTransRot(center, rot);
            
			foreach (int i in Enumerable.Range(0, elevations.Count() - 2))
            {
                foreach (int j in Enumerable.Range(0, azimuths.Count() - 1))
                {
                    Vector3 p1 = CelestialToCartesian(azimuths.ElementAt(j),
                        elevations.ElementAt(i), radius);

					Vector3 p2 = CelestialToCartesian(azimuths.ElementAt(j),
                        elevations.ElementAt(i + 1), radius);

					Vector3 p3 = CelestialToCartesian(azimuths.ElementAt(j + 1),
                        elevations.ElementAt(i + 1), radius);

					Vector3 p4 = CelestialToCartesian(azimuths.ElementAt(j + 1),
                        elevations.ElementAt(i), radius);

                    AddVert(p1);
                    AddVert(p2);
                    AddVert(p3);
                    AddVert(p4);
                    AddLastVertsAsQuad();
                }
            }
            foreach (int k in Enumerable.Range(0, azimuths.Count() - 1))
            {
				Vector3 p1 = CelestialToCartesian(azimuths.ElementAt(k),
                    elevations.ElementAt(elevations.Count() - 2), radius);

                Vector3 p2 = new Vector3(0, radius, 0);

				Vector3 p3 = CelestialToCartesian(azimuths.ElementAt(k + 1),
                    elevations.ElementAt(elevations.Count() - 2), radius);

                AddVert(p1);
                AddVert(p2);
                AddVert(p3);
                AddLastVertsAsTri();
            }
            int endVerts = newVerts.Count;
			AddRepeatingAttribute<Color>(newColors, color, endVerts - startVerts);
            return this;
        }

        /// <summary>
        /// Adds an AvaraBSP from special JSON format.
        /// </summary>
        /// <returns>The avara BSP.</returns>
        /// <param name="json">Json.</param>
		/// <param name="marker1">Marker 1 color (for replacing).</param>
		/// <param name="marker2">Marker 2 color (for replacing).</param>
        public GeomBuilder AddAvaraBSP(string json, Color marker1, Color marker2)
        {
            AvaraBSP bsp = new AvaraBSP(json);
            int count = 0;
            foreach (PolyRecord p in bsp.polys)
            {
                AddAvaraBSPPoly(bsp, p, count, marker1, marker2);
                count++;
            }

            return this;
        }

        private void AddAvaraBSPPoly(AvaraBSP bsp, PolyRecord p, int idx, Color marker1, Color marker2)
        {
			var normalRec = bsp.normalRecords[p.normalIndex];
			var basePoint = normalRec.basePointIndex;

			var normal = (Vector3)bsp.vectors[normalRec.normalIndex];
            normal.Normalize();

			Color theColor = (Color)bsp.colors[normalRec.colorIndex];
            // TODO: This isn't quite right yet for some reason, 
            // i think there are more than two marker colors
            if ( // marker 1
                theColor.r == 1 &&
                theColor.g == 1 &&
                theColor.b == 0 &&
                theColor.a == 1
                )
            {
                theColor = marker1;
            }

            if ( // marker 2
                theColor.r == 0 &&
                theColor.g == 1 &&
                theColor.b == 0 &&
                theColor.a == 0
               )
            {
                theColor = marker2;
            }

            var verts = bsp.trianglesVerts[idx];
            var triangles = bsp.triangles[idx];
            var points = verts.Select(x => bsp.points[x]);

            foreach (Triangle tri in triangles)
            {
                AvaraBSPVertsTri(new List<Vector3>() {
                points.ElementAt(tri.a),
                points.ElementAt(tri.b),
                points.ElementAt(tri.c)
            }, bsp, normal, theColor);
            }
        }

		void AvaraBSPVertsTri(IEnumerable<Vector3> verts, AvaraBSP bsp, Vector3 normal, Color c)
        {
			var startVert = newVerts.Count;
			newVerts.AddRange(verts);
			var endVert = newVerts.Count;

            newTriangles.Add(startVert + 2);
			newTriangles.Add(startVert + 1);
			newTriangles.Add(startVert);

            AddRepeatingAttribute<Vector3>(newNormals, normal, endVert - startVert);
            AddRepeatingAttribute<Color>(newColors, c, endVert - startVert);

            startVert = newVerts.Count;
            newVerts.AddRange(verts);
            endVert = newVerts.Count;

            newTriangles.Add(startVert);
            newTriangles.Add(startVert + 1);
            newTriangles.Add(startVert + 2);

			AddRepeatingAttribute<Vector3>(newNormals, normal, endVert - startVert);
            AddRepeatingAttribute<Color>(newColors, c, endVert - startVert);
        }
	}
}