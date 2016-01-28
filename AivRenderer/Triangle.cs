using System;
using Aiv.Draw;
using Aiv.Math;

namespace AivRenderer
{
	public class Triangle
	{
		public Vertex a;
		public Vertex b;
		public Vertex c;

		// every triangle needs a mapping to a mesh
		private Mesh mesh;


		public Triangle (Mesh mesh, Vertex a, Vertex b, Vertex c)
		{
			this.a = a;
			this.b = b;
			this.c = c;

			this.mesh = mesh;
		}

		// draw a 3d triangle in a 2d space
		public void Draw (Camera camera)
		{

			// transform local coordinates in world coordinates
			// remember: scaling is vector multiplication, translation is vector addition
			Vector3 aWorld = this.a.coordinates.Rotate(mesh.rotationEuler) * mesh.scale + mesh.position;
			Vector3 bWorld = this.b.coordinates.Rotate(mesh.rotationEuler) * mesh.scale + mesh.position;
			Vector3 cWorld = this.c.coordinates.Rotate(mesh.rotationEuler) * mesh.scale + mesh.position;

			// apply camera trasnformations (remember they are always inverted)
			Vector3 aCamera = aWorld - (camera.position);
			Vector3 bCamera = bWorld - (camera.position);
			Vector3 cCamera = cWorld - (camera.position);

			// project 3d vertices (in camera space) on 2d surface
			this.a.projected = aCamera.Project (camera).NDCtoPixel ();
			this.b.projected = bCamera.Project (camera).NDCtoPixel ();
			this.c.projected = cCamera.Project (camera).NDCtoPixel ();


			// now we have 2d triangles, time to rasterize them
			// first step is ordering vertices from top to bottom


			// order vertices (using projected coordinates [x and y are pixels, z is a copy of the camera transformation])
			Vertex p1 = this.a;
			Vertex p2 = this.b;
			Vertex p3 = this.c;

			// we use a dumb swapping algorithm for performances
			if (p1.projected.y > p2.projected.y) {
				var tmp = p2;
				p2 = p1;
				p1 = tmp;
			}
			if (p2.projected.y > p3.projected.y) {
				var tmp = p2;
				p2 = p3;
				p3 = tmp;
			}
			if (p1.projected.y > p2.projected.y) {
				var tmp = p2;
				p2 = p1;
				p1 = tmp;
			}
				
			// find slopes of the triangle edges, it is required to understand if P2 is on the left or on the right
			float slopeP1P2 = (p2.projected.x - p1.projected.x) / (p2.projected.y - p1.projected.y);
			float slopeP1P3 = (p3.projected.x - p1.projected.x) / (p3.projected.y - p1.projected.y);

			// iterate y from p1 to p3
			for (int y = (int)p1.projected.y; y <= (int)p3.projected.y; y++) {
				// p2 on the left
				if (slopeP1P3 > slopeP1P2) {
					if (y < p2.projected.y) {
						Device.ScanLine (y, p1, p2, p1, p3, mesh.texture);
					} else {
						Device.ScanLine (y, p2, p3, p1, p3, mesh.texture);
					}
				} else {

					//p2 on the right
					if (y < p2.projected.y) {
						Device.ScanLine (y, p1, p3, p1, p2, mesh.texture);
					} else {
						Device.ScanLine (y, p1, p3, p2, p3, mesh.texture);
					}
				}
			}

		}
			
	}
}

