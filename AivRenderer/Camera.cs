using System;
using Aiv.Draw;
using Aiv.Math;

namespace AivRenderer
{
	public class Camera
	{

		public Vector3 position;

		public float fov { get; private set; }

		public Camera (float fov, Vector3 position)
		{
			
			this.position = position;
			this.fov = fov;
		}
	}
}

