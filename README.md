# MassRotate

MassRotate allows you rotate or scale a GFX file multiple times.
The concept is pretty simple, it takes a GFX file, the user
inputs the size of the sprite in the GFX file (note that it must
be the very first tile in the GFX), if the tool must rotate or
scale it, the amount of frames to generate and the total amount
of degress (or the new sprite/gfx size).

It is a Console application, but it should be easy enough for
anyone with average computer experience use it. Open your prompt
command and go to the folder where Mass Rotate is located.

Alternatively, depending on where Mass Rotate is located, you can
simply press Shift + Right Click on Windows Explorer and select
"Open command line here" or something like.

----------------------------------------------------------------

Usage (for rotating GFX):

mass rotate &lt;format&gt; &lt;frames&gt; &lt;size&gt; &lt;degress&gt; &lt;input&gt; &lt;output&gt;

&lt;format>: The GFX format. The following formats are supported:
	- 2: 2BPP GB.
	- 3: 3BPP SNES, rarely used.
	- 4: 4BPP SNES.
	- 7: Mode 7 GFX, non-interleaved.
	- 8: 8BPP SNES.

&lt;frames&gt;: The amount of frames to generate while rotating.

&lt;size&gt;: The size of the input GFX. If your sprite for example
is 32x32, you put 32 on this parameter.

&lt;degress&gt;: The total amount of degress to rotate.

&lt;input&gt;: The input file, like "mushroom.bin". Yes, you must
type the file extension.

&lt;output&gt;: The output file. Again, you must type the file extension.

Example:
You have a 16x16 mushroom, you want to generate 8 frames with a
rotation of 180° and your file format is 4BPP SNES, you'd type:

mass rotate 4 8 16 180 "mushroom.bin" "new_mushroom.bin"

----------------------------------------------------------------

Usage (for down-scaling GFX):

mass scale &lt;format&gt; &lt;frames&gt; &lt;size&gt; &lt;new_size&gt; &lt;input&gt; &lt;output&gt;

&lt;frames&gt;: The amount of frames to generate while scaling.

&lt;size&gt;: The size of the input GFX. If your sprite for example is
64x64, you'd put 64 here.

&lt;new_size&gt;: The size to scale. If you want to your sprite reduce
to 16x16, simply put 16 here.

&lt;input&gt;: The input file, like "mushroom.bin". Yes, you must
type the file extension.

&lt;output&gt;: The output file. Again, you must type the file extension.

Example:
You have a 64x64 question block and you want to generate 16 frames
while reducing its size to 32x32 and your file format is 8BPP SNES:

mass scale 8 16 64 32 "question.bin" "new_question.bin"

----------------------------------------------------------------

Usage (for up-scaling GFX):

The process is exactly same as down-scaling, but there's something
important you should know before doing that.

The area where your graphics will be in must have the size of the
scaled area and your GFX must be centered on that area. What do
you mean?

You have a 16x16 shell and you want to scale it to 64x64. Alright,
in the YY-CHR, select a 64x64 area, clear and paste your shell.
Then, move your shell 32 pixels to right and 32 pixels to down.

Instead of putting the <size> parameter, you must put 64 instead
and in the <new_size> parameter, you must put 256 instead of 64.

So instead of

mass scale <format> <frames> 16 64 "file.bin" "new_file.bin"

You put:

mass scale <format> <frames> 64 256 "file.bin" "new_file.bin"

Why 256 for <new_size>? Because you're scaling it four times.
64/16 = 4; Size <size> is now 64, then you must multiply new
size by 4. So hence why 256.

----------------------------------------------------------------

That's all. If you have a question about this, feel free to PM
me, Vitor Vilela on SMWC or alternatively, ask the SMWC forums.
