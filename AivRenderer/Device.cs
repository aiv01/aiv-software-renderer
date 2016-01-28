using System;
using Aiv.Draw;
using Aiv.Math;

namespace AivRenderer
{
	/*
	 * 
	 * 
	 * There is a single Device in the program
	 * 
	 * you can see it as the graphics card in the system
	 * 
	*/
	public static class Device
	{
		// the Aiv.Draw window
		public static Window window;

		// there is a single z-buffer per-device (it is independent by the number of cameras)
		public static float[] zBuffer;

		// aspect ration of the window (required for projection)
		public static float aspectRatio;

		public static void Init (int width, int height, string title)
		{
			// avoid multiple initializations
			if (window != null)
				return;

			// create Aiv.Draw window
			window = new Window (width, height, title, PixelFormat.RGB);
			// create the z-buffer
			zBuffer = new float[window.width * window.height];

			aspectRatio = (float)window.width / (float)window.height;
		}

		// proxy member to access window status
		public static bool Active { get { return window.opened; } }

		// proxy method for updating time and drawing
		public static void Update ()
		{
			window.Blit ();
		}

		// light-on a pixel in the window bitmap, z is the value used for the z-buffer
		// while x and y are in pixel corrdinates, z is the interpolated value taken by the vertex after camera transform
		// (and before projection)
		public static void PutPixel (int x, int y, float z, byte r, byte g, byte b)
		{
			// ensure pixel is not outside the window
			if (x < 0 || x > window.width - 1 || y < 0 || y > window.height - 1)
				return;
			
			// find the pixel position in the z-buffer
			int zpos = (y * window.width) + x;
			// if the value in the z-buffer is lower than the new pixel, skip drawing
			if (zBuffer [zpos] < z)
				return;

			// set the new z-buffer value
			zBuffer [zpos] = z;

			// find pixel position (given x and y) in the window bitmap array (we use '3' as the window is RGB)
			int pos = (y * 3 * window.width) + (x * 3);
			window.bitmap [pos] = r;
			window.bitmap [pos + 1] = g;
			window.bitmap [pos + 2] = b;
		}

		// clear the window and the zbuffer
		// here we do not use normalized colors as we are in the device "world"
		public static void Clear (byte r, byte g, byte b)
		{
			// fill pixels of teh window (RGB, so we use 3 as the delta)
			for (int i = 0; i < window.bitmap.Length; i += 3) {
				window.bitmap [i] = r;
				window.bitmap [i + 1] = g;
				window.bitmap [i + 2] = b;
			}
				
			for (int i = 0; i < zBuffer.Length; i++)
				zBuffer [i] = float.MaxValue;

		}

		// proxy method for input management
		public static bool GetKey (KeyCode kc)
		{
			return window.GetKey (kc);
		}

		// two commodity constants (like in Unity3D)
		public static float Deg2Rad = (float)(Math.PI / 180f);
		public static float Rad2Deg = (float)(180f / Math.PI);

		// dumb extension method for float allowing clamp between 0 and 1
		public static float Clamp (this float value)
		{
			if (value < 0)
				return 0;
			if (value > 1)
				return 1;
			return value;
		}

		// extension method for float linear interpolation
		public static float Interpolate (this float value, float max, float gradient)
		{
			return value + (max - value) * gradient.Clamp ();
		}

		// rasterization procedure
		public static void ScanLine (int y, Vertex vertexLeftTop, Vertex vertexLeftBottom, Vertex vertexRightTop, Vertex vertexRightBottom, Texture texture)
		{
			// assumes a flat triangle
			float gradientLeft = 1;
			float gradientRight = 1;

			// non flat triangle ?
			if (vertexLeftTop.projected.y != vertexLeftBottom.projected.y) {
				gradientLeft = (y - vertexLeftTop.projected.y) / (vertexLeftBottom.projected.y - vertexLeftTop.projected.y);
			}

			if (vertexRightTop.projected.y != vertexRightBottom.projected.y) {
				gradientRight = (y - vertexRightTop.projected.y) / (vertexRightBottom.projected.y - vertexRightTop.projected.y);
			}

			// find x start position and end position using the y gradient
			int left = (int)vertexLeftTop.projected.x.Interpolate (vertexLeftBottom.projected.x, gradientLeft);
			int right = (int)vertexRightTop.projected.x.Interpolate (vertexRightBottom.projected.x, gradientRight);

			// find the z start and value using the same interpolation
			float zStart = vertexLeftTop.projected.z.Interpolate (vertexLeftBottom.projected.z, gradientLeft);
			float zEnd = vertexRightTop.projected.z.Interpolate (vertexRightBottom.projected.z, gradientRight);

			// get U start and end
			float uStart = vertexLeftTop.uv.x.Interpolate (vertexLeftBottom.uv.x, gradientLeft);
			float uEnd = vertexRightTop.uv.x.Interpolate (vertexRightBottom.uv.x, gradientRight);

			// get V start and end
			float vStart = vertexLeftTop.uv.y.Interpolate (vertexLeftBottom.uv.y, gradientLeft);
			float vEnd = vertexRightTop.uv.y.Interpolate (vertexRightBottom.uv.y, gradientRight);


			for (int x = left; x < right; x++) {

				// while we do not need interpolation for horizontal pixels
				// we need it for texturing and for getting correct z value
				// zGradient name can be a bit misleading, as it will be used for UV's too
				float zGradient = ((float)x - (float)left) / ((float)right - (float)left);

				// get the z value using interpolation
				float z = zStart.Interpolate (zEnd, zGradient);

				// find texture uv coordinates using interpolation
				// we can resue the zGradient
				float u = uStart.Interpolate (uEnd, zGradient);
				float v = vStart.Interpolate (vEnd, zGradient);

				// get texture pixel color using UV system
				// note the (1f - v), it is required as V coordinate is reversed (0 on bottom, 1 on top)
				Vector3 color = texture.Map (u, 1f - v);

				// write pixel on the device, de-normalizing colors
				PutPixel (x, (int)y, z,
					(byte)(color.x * 255),
					(byte)(color.y * 255),
					(byte)(color.z * 255));
			}

		}
			

		// Vector3 extension methods

		public static Vector3 RotateY (this Vector3 v, float amount)
		{
			Vector3 v3;
			float rad = amount * Deg2Rad;
			v3.x = v.x * (float)Math.Cos (rad) + v.z * (float)Math.Sin (rad);
			v3.y = v.y;
			v3.z = -v.x * (float)Math.Sin (rad) + v.z * (float)Math.Cos (rad);
			return v3;
		}

		public static Vector3 RotateX (this Vector3 v, float amount)
		{
			Vector3 v3;
			float rad = amount * Deg2Rad;
			v3.x = v.x;
			v3.y = v.y * (float)Math.Cos (rad) - v.z * (float)Math.Sin (rad);
			v3.z = v.y * (float)Math.Sin (rad) + v.z * (float)Math.Cos (rad);
			return v3;
		}

		public static Vector3 RotateZ (this Vector3 v, float amount)
		{
			Vector3 v3;
			float rad = amount * Deg2Rad;
			v3.x = v.x * (float)Math.Cos (rad) - v.y * (float)Math.Sin (rad);
			v3.y = v.x * (float)Math.Sin (rad) + v.y * (float)Math.Cos (rad);
			v3.z = v.z;
			return v3;
		}

		// commodity function, it simply combine rotations
		public static Vector3 Rotate (this Vector3 v, Vector3 rot)
		{
			// use YXZ ordering
			return v.RotateY(rot.y).RotateX(rot.x).RotateZ(rot.z);
		}

		// the Project method returns NDC coordinated, let's convert them to pixels
		public static Vector3 NDCtoPixel (this Vector3 v)
		{
			// cast window sizes to float to avoid rounding errors
			float w = window.width;
			float h = window.height;

			Vector3 v3;
			v3.x = (v.x * w / 2f) + (w / 2f);
			v3.y = -(v.y * h / 2f) + (h / 2f);
			v3.z = v.z;

			return v3;
		}

		// apply perspective projection to vector using the specified camera
		public static Vector3 Project (this Vector3 v, Camera camera)
		{
			Vector3 v3;
			float angle = camera.fov / 2 * Deg2Rad;

			// 1f/tan(alpha)
			float distanceFromCamera = (float)Math.Tan (angle);

			v3.y = v.y / (distanceFromCamera * v.z);
			v3.x = v.x / (aspectRatio * distanceFromCamera * v.z);
			// hold the original z value, as it will be used by z-buffering
			v3.z = v.z;

			return v3;
		}

	}
}

