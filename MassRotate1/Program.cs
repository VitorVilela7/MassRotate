using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace MassRotate
{
	unsafe class Program
	{
		private static void ExitError(string reason, params string[] format)
		{
			Console.WriteLine(reason.Replace("\n", Environment.NewLine), format);

			Console.Write("Press any key to exit... ");
			Console.ReadKey(true);
			Console.WriteLine();

			Environment.Exit(-1);
		}

		private static float ParseFloat(string value)
		{
			try
			{
				return Single.Parse(value, CultureInfo.InvariantCulture);
			}
			catch
			{
				ExitError("Can't parse value: {0}", value);
				return 0;
			}
		}

		private static int ParseInteger(string value)
		{
			try
			{
				return Int16.Parse(value, CultureInfo.InvariantCulture);
			}
			catch
			{
				ExitError("Can't parse value: {0}", value);
				return 0;
			}
		}

		private static void Main(string[] args)
		{
			SnesGFX.SnesGFX.Init();

			Console.WriteLine("MassRotate v1.00");
			Console.WriteLine(" By Vitor Vilela");
			Console.WriteLine("----------------");
			Console.WriteLine();

			// if scaling:
			// mass scale <format> <frames> <size> <new_size> <input file> <output file>
			// 9 args

			// if rotating:
			// mass rotate <format> <frames> <size> <total degress> <input file> <output file>
			// 7 args

			if (args.Length != 7)
			{
				ExitError("Invalid amount of paramters.\nPlease take a look on the readme for more information.");
			}

			int gfxFormat = ParseInteger(args[1].ToLower().Replace("bpp", ""));

			switch (gfxFormat)
			{
				case 2: gfxFormat = 0; break;
				case 3: gfxFormat = 1; break;
				case 4: gfxFormat = 2; break;
				case 8: gfxFormat = 3; break;
				case 7: gfxFormat = 9; break; // because why not
				default: ExitError("Unknown GFX format specified: {0}", args[1]); break;
			}

			float max = ParseFloat(args[4]);
			int frames = ParseInteger(args[2]);
			int width = ParseInteger(args[3]);
			string inputFile = args[5];
			string outputFile = args[6];

			int centerX = width >> 1;
			int centerY = width >> 1;

			bool scale;
			byte[] input, output;

			switch (args[0].ToLower())
			{
				case "scale":
					scale = true;
					break;

				case "rotate":
					scale = false;
					break;

				default:
					ExitError("Invalid format.\nPlease take a look on the readme for more information.");
					return;
			}

			if (!File.Exists(inputFile))
			{
				ExitError("File not found: {0}", inputFile);
			}

			try
			{
				input = File.ReadAllBytes(inputFile);
			}
			catch (Exception ex)
			{
				ExitError("Couldn't read file: {0}", ex.Message);
				return;
			}

			try
			{
				if (scale)
				{
					output = MassScaleSNES(input, max, frames, width, centerX, centerY, gfxFormat);
				}
				else
				{
					output = MassRotateSNES(input, max, frames, width, gfxFormat);
				}
			}
			catch (Exception ex)
			{
				ExitError("Error while processing GFX: {0}", ex.Message);
				return;
			}

			try
			{
				File.WriteAllBytes(outputFile, output);
			}
			catch (Exception ex)
			{
				ExitError("Error while saving output file: {0}", ex.Message);
			}

			//lulz
			//ExitError("No error.");
		}

		private static byte[] MassScaleSNES(byte[] input, float max,
			int frames, int snesWidth, int centerX, int centerY, int format)
		{
			int mmax = (int)frames;

			if (mmax % snesWidth != 0)
			{
				mmax += snesWidth - mmax % snesWidth;
			}

			float dt = (max - snesWidth) / (float)(frames - 1);
			int size;

			byte[] rawBmpData = SnesGFX.SnesGFX.AvaiableFormats[format].Decode(input);

			using (Bitmap bmp = new Bitmap(snesWidth, snesWidth, PixelFormat.Format8bppIndexed))
			{
				var palette = bmp.Palette;
				for (int i = 0; i < 256; ++i)
				{
					palette.Entries[i] = Color.FromArgb(i, i, i);
				}
				bmp.Palette = palette;
				var bmpData = bmp.LockBits(new Rectangle(0, 0, snesWidth, snesWidth),
					ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

				for (int x = 0; x < snesWidth; ++x)
				{
					for (int y = 0; y < snesWidth; ++y)
					{
						int off = y * snesWidth + x;
						int off2 = y * 128 + x;
						*(((byte*)bmpData.Scan0) + off) = rawBmpData[off2];
					}
				}
				bmp.UnlockBits(bmpData);

				switch (bmp.Size.Width)
				{
					case 8:
					case 16:
					case 32:
					case 64:
					case 128:
						size = bmp.Size.Width;
						break;

					default:
						throw new Exception("The bitmap must have a width of 8, 16, 32, 64 or 128 pixels.");
				}

				using (Bitmap data = new Bitmap(size, size, PixelFormat.Format32bppArgb))
				using (Bitmap output = new Bitmap(128, (size * mmax) / (128 / size), PixelFormat.Format32bppArgb))
				{
					using (Graphics g = Graphics.FromImage(output))
					using (Graphics original = Graphics.FromImage(data))
					{
						original.SmoothingMode = g.SmoothingMode = SmoothingMode.HighQuality;
						original.InterpolationMode = g.InterpolationMode = InterpolationMode.NearestNeighbor;
						original.PixelOffsetMode = g.PixelOffsetMode = PixelOffsetMode.HighQuality;

						float rotation = snesWidth;
						float size1x = centerX;
						float size2x = -size1x;
						float size1y = centerY;
						float size2y = -size1y;

						int z = 0;

						for (int x = 0, y = (size * mmax) / (128 / size); x < y; x++)
						{
							for (int i = 0, j = (128 / size); i < j; i++)
							{
								original.Clear(Color.Transparent);

								if (rotation / (float)snesWidth != 0)
								{
									original.ResetTransform();
									original.TranslateTransform(size1x, size1y);
									original.ScaleTransform(rotation / (float)snesWidth, rotation / (float)snesWidth);
									original.TranslateTransform(size2x, size2y);
									original.DrawImage(bmp, 0, 0, size, size);

									Console.WriteLine("frame {0,3}, new size: {1}x{1}", z, rotation);
								}

								g.DrawImage(data, size * i, x * size, size, size);

								if (++z == frames)
								{
									goto breakLoop;
								}

								rotation = snesWidth + dt * z;
							}
						}
					}


				breakLoop:
					bmpData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height),
						ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

					var pixelData = new byte[output.Width * output.Height * 4];
					Marshal.Copy(bmpData.Scan0, pixelData, 0, pixelData.Length);

					var correctData = new byte[output.Width * output.Height];

					for (int i = 0; i < pixelData.Length; i += 4)
					{
						correctData[i >> 2] = pixelData[i + 1];
					}

					byte[] outputData = SnesGFX.SnesGFX.AvaiableFormats[format].Encode(correctData);
					output.UnlockBits(bmpData);

					return outputData;
				}
			}
		}

		private static byte[] MassRotateSNES(byte[] db, float max, int frames, int snesWidth, int format)
		{
			int mmax = (int)frames;
			int size;

			max = max / 360.0f;

			float dt = 360f / (float)frames * max;
			byte[] rawBmpData = SnesGFX.SnesGFX.AvaiableFormats[format].Decode(db);

			using (Bitmap bmp = new Bitmap(snesWidth, snesWidth, PixelFormat.Format8bppIndexed))
			{
				var palette = bmp.Palette;
				for (int i = 0; i < 256; ++i)
				{
					palette.Entries[i] = Color.FromArgb(i, i, i);
				}
				bmp.Palette = palette;
				var bmpData = bmp.LockBits(new Rectangle(0, 0, snesWidth, snesWidth),
					ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

				for (int x = 0; x < snesWidth; ++x)
				{
					for (int y = 0; y < snesWidth; ++y)
					{
						*(((byte*)bmpData.Scan0) + y * snesWidth + x) = rawBmpData[y * 128 + x];
					}
				}

				bmp.UnlockBits(bmpData);

				if (bmp.Size.Width % 8 != 0)
				{
					throw new Exception("The Bitmap must be multiple of 8.");
				}

				size = bmp.Size.Width;

				int safe = 128 - (128 % size);

				using (Bitmap data = new Bitmap(size, size, PixelFormat.Format32bppArgb))

				using (Bitmap output = new Bitmap(128, (size * mmax) / (safe / size), PixelFormat.Format32bppArgb))
				{
					using (Graphics g = Graphics.FromImage(output))

					using (Graphics original = Graphics.FromImage(data))
					{
						original.SmoothingMode = g.SmoothingMode = SmoothingMode.HighQuality;
						original.InterpolationMode = g.InterpolationMode = InterpolationMode.NearestNeighbor;
						original.PixelOffsetMode = g.PixelOffsetMode = PixelOffsetMode.HighQuality;

						float rotation = 0;

						float size1 = (float)size / 2f;
						float size2 = -size1;

						int z = 0;

						for (int x = 0, y = (size * mmax) / (safe / size); x < y; x++)
						{
							for (int i = 0, j = (safe / size); i < j; i++)
							{
								original.Clear(Color.Transparent);
								original.ResetTransform();
								original.TranslateTransform(size1, size1);
								original.RotateTransform(rotation);
								original.TranslateTransform(size2, size2);
								original.DrawImage(bmp, 0, 0, size, size);
								g.DrawImage(data, size * i, x * size, size, size);
								Console.WriteLine("frame {0,3}, total rotate: {1} degress.", z, rotation);

								if (++z == frames)
								{
									goto finish;
								}

								rotation = dt * z;
							}
						}

					}

				finish:
					bmpData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height),
						ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

					var pixelData = new byte[output.Width * output.Height * 4];
					Marshal.Copy(bmpData.Scan0, pixelData, 0, pixelData.Length);

					var correctData = new byte[Math.Max(8192, output.Width * output.Height)];

					for (int i = 0; i < pixelData.Length; i += 4)
					{
						correctData[i >> 2] = pixelData[i + 1];
					}

					byte[] outputData = SnesGFX.SnesGFX.AvaiableFormats[format].Encode(correctData);
					output.UnlockBits(bmpData);

					return outputData;
				}
			}
		}
	}
}
