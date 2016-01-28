using System;
using Aiv.Draw;
using Aiv.Math;

namespace AivRenderer
{
	public class Texture
	{

		private Sprite image;

		public Texture (string fileName)
		{
			this.image = new Sprite (fileName);
		}

		// returns the normalized color (as Vector3), given uv coordinates
		public Vector3 Map (float u, float v)
		{
			// the modulo operators avoid value to go out of the image bitmap
			int x = Math.Abs ((int)(u * image.width) % image.width);
			int y = Math.Abs ((int)(v * image.height) % image.height);

			// get the pixel position in the image array
			// we use 4 are sprites are stored as RGBA
			int pos = (y * image.width * 4) + (x * 4);

			byte r = image.bitmap [pos];
			byte g = image.bitmap [pos + 1];
			byte b = image.bitmap [pos + 2];

			// normalize color components
			return new Vector3 (
				(float)r / 255f,
				(float)g / 255f,
				(float)b / 255f
			);
		}
	}
}

