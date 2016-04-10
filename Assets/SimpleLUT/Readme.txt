Simple LUT Adjuster
(c) 2015 Digital Ruby, LLC
http://www.digitalruby.com/unity-plugins

I'm Jeff Johnson, and I made this simple LUT color correction script to allow you to rapidly see color correction changes in your Unity scene.

Instructions:
- Drag the SimpleLUT script on to your main camera. The script runs in editor mode so there is no need to run your scene to see changes.
- The script has a number of properties that can be changed, including:
	- Shader: The built in shader (SimpleLUT.shader) should generally not be modified or removed.
	- Lookup Texture: The color correction lookup texture. This needs to be power of 2 dimensions, height must be equal to square root of width. Read/write must be enabled and no mipmaps. A few sample LUT textures are included.
	- Amount: Value from 0 - 1, controls the weight of color correction where 0 is no color correction and 1 is full color correction.
	- Tint Color: Apply a tint color to the final pixel.
	- Hue: Change the hue (0 - 360).
	- Saturation: Value from -1 to 1, controls how much color is added, where -1 is all gray, 0 is normal and 1 is extra color.
	- Brightness: Value from -1 to 1, where -1 is all black, 0 is normal and 1 is full brightness.
	- Contrast: Value from -1 to 1 where -1 is no contrast, 0 is normal and 1 is full contrast.
	- Sharpness: Value from 0 to 1 where 0 is no sharpness and 1 is some sharpness.
- Your target device will need to support 3D textures in order to do the color correction.

Please email support@digitalruby.com if you have any questions, feature requests or bug reports.

Thank you.

- Jeff Johnson, creator of Simple LUT