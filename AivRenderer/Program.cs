using System;
using Aiv.Draw;
using Aiv.Math;

namespace AivRenderer
{
	class MainClass
	{

		/*
		 * 
		 * 
		 * TODO near/far
		 * Rotations and full camera (with rotations)
		 * Matrices
		 * Better line drawing and Rasterization
		 * Lights and shading
		 * 
		 * 
		 * 
		 * 
		 * 
		 * 
		 * 
		 */

		public static void Main (string[] args)
		{

			Device.Init (1024, 576, "Software Renderer");

			// initialize a camera with 60 degrees field of view
			// the second argument is the position of the camera (left handed)
			Camera camera = new Camera (60, new Vector3 (0, 0, -10));

			// load the storm trooper texture
			Texture stormTrooperTexture = new Texture("../../Assets/Stormtrooper.png");

			// load the stormtrooper mesh, and apply the specified texture
			Mesh stormTrooper = new Mesh ("../../Assets/Stormtrooper.obj", stormTrooperTexture);

			// scale in half
			stormTrooper.scale = new Vector3 (0.5f, 0.5f, 0.5f);
			// place y at -1 to have better view of the storm tropper
			stormTrooper.position = new Vector3 (0, -1, 0);
			// start with a 0 rotation
			stormTrooper.rotationEuler = new Vector3 (0, 0, 0);

			float z = camera.position.z;
			float y = stormTrooper.rotationEuler.y;

			while (Device.Active) {
				// red background (remember, colors are not normalized in device space)
				Device.Clear (255, 0, 0);

				// input management, we rotate the mesh with left and right, and we zoom in/out with up and down
				if (Device.GetKey (KeyCode.Right))
					y+=3;

				if (Device.GetKey (KeyCode.Left))
					y-=3;

				if (Device.GetKey (KeyCode.Up))
					z+=1;

				if (Device.GetKey (KeyCode.Down))
					z-=1;

				// set z of the camera
				camera.position = new Vector3 (0, 0, z);

				// apply rotation to the model
				stormTrooper.rotationEuler = new Vector3(0, y, 0);

				// draw the model
				stormTrooper.Draw (camera);

				// update the screen
				Device.Update ();
			}
		}
	}
}
