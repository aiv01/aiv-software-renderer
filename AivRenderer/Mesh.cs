using System;
using System.Collections.Generic;
using System.IO;
using Aiv.Draw;
using System.Globalization;
using Aiv.Math;

namespace AivRenderer
{
	public class Mesh
	{

		// all public as the program is allowed to change them
		public Vector3 position;
		public Vector3 rotationEuler;
		public Vector3 scale;

		// internal lists for obj parser
		private List<Vector3> vertices;
		private List<Vector2> uvs;
		private List<Vector3> normals;

		// the list of object faces
		private List<Triangle> faces;

		// obj format allows a single texture per mesh
		public Texture texture { get; private set;}

		public Mesh (string fileName, Texture texture)
		{

			this.position = Vector3.zero;
			this.rotationEuler = Vector3.zero;
			// initialize with default scaling
			this.scale = new Vector3(1, 1, 1);

			this.vertices = new List<Vector3> ();
			this.uvs = new List<Vector2> ();
			this.normals = new List<Vector3> ();
			this.faces = new List<Triangle> ();

			this.texture = texture;

			using (StreamReader reader = new StreamReader (fileName)) {
				
				while (true) {
					string line = reader.ReadLine ();
					if (line == null)
						break;

					// manage vertices
					if (line.StartsWith ("v ")) {
						string[] items = line.Split (' ');
						// check for right handed and left handed !!!
						this.vertices.Add (new Vector3 (
							float.Parse (items [1], CultureInfo.InvariantCulture),
							float.Parse (items [2], CultureInfo.InvariantCulture),
							float.Parse (items [3], CultureInfo.InvariantCulture) * -1
						));
					}

					// manage texture coordinates (uv)
					if (line.StartsWith ("vt ")) {
						string[] items = line.Split (' ');
						this.uvs.Add (new Vector2 (
							float.Parse (items [1], CultureInfo.InvariantCulture),
							float.Parse (items [2], CultureInfo.InvariantCulture)
						));
					}

					// manage normals
					if (line.StartsWith ("vn ")) {
						string[] items = line.Split (' ');
						// check for right handed and left handed !!!
						this.normals.Add (new Vector3 (
							float.Parse (items [1], CultureInfo.InvariantCulture),
							float.Parse (items [2], CultureInfo.InvariantCulture),
							float.Parse (items [3], CultureInfo.InvariantCulture)
						));
					}

					if (line.StartsWith ("f ")) {
						string[] items = line.Split (' ');
						string []id1 = items [1].Split ('/');
						string []id2 = items [2].Split ('/');
						string []id3 = items [3].Split ('/');

						// vertex
						Vector3 av = this.vertices [int.Parse(id1[0]) - 1];
						Vector3 bv = this.vertices [int.Parse(id2[0]) - 1];
						Vector3 cv = this.vertices [int.Parse(id3[0]) - 1];

						// uv
						Vector2 auv = this.uvs [int.Parse(id1[1]) - 1];
						Vector2 buv = this.uvs [int.Parse(id2[1]) - 1];
						Vector2 cuv = this.uvs [int.Parse(id3[1]) - 1];

						// normal
						Vector3 an = this.normals [int.Parse(id1[2]) - 1];
						Vector3 bn = this.normals [int.Parse(id2[2]) - 1];
						Vector3 cn = this.normals [int.Parse(id3[2]) - 1];

						Vertex a = new Vertex (av, auv, an);
						Vertex b = new Vertex (bv, buv, bn);
						Vertex c = new Vertex (cv, cuv, cn);

						this.faces.Add (new Triangle (this, a, b, c));

					}
				}
			}

		}

		public void Draw (Camera camera)
		{
			foreach (Triangle face in this.faces) {
				face.Draw (camera);
			}
		}
	}
}

