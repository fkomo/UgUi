﻿using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Types
{
	[NodeInfo]
	public class Bitmap : NodeOperationBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true)]
		public v4[] Color { get; set; }

		private v2 size;
		[Input(Order = 1, InputAnchor = true, AnchorOnly = true)]
		public v2 Size 
		{ 
			get { return size; }
			set 
			{ 
				SetField(ref size, value, nameof(Size)); 
				OnPropertyChanged($"{ nameof(Size) }{ nameof(Size.X) }");
				OnPropertyChanged($"{ nameof(Size) }{ nameof(Size.Y) }");
			}
		}

		private BitmapSource output = null;
		[Output(NoAnchor = true, DisplayName = "", ReadOnly = true)]
		public BitmapSource Output 
		{
			get { return output; }
			set { SetField(ref output, value, nameof(Output)); } 
		}

		public override void Execute()
		{
			if (Color != null && Size != null && Size.X * Size.Y == Color.Length)
			{
				var pixels = new byte[(int)Size.X * (int)Size.Y * 4];
				for (var i = 0; i < Color.Length; i++)
				{
					pixels[i * 4 + 0] = (byte)(Color[i].X * 255);
					pixels[i * 4 + 1] = (byte)(Color[i].Y * 255);
					pixels[i * 4 + 2] = (byte)(Color[i].Z * 255);
					pixels[i * 4 + 3] = (byte)(Color[i].W * 255);
				}

				Output = BitmapSource.Create((int)Size.X, (int)Size.Y, 96, 96, PixelFormats.Bgra32, null, pixels, (int)Size.X * 4);
			}
			else
				Output = null;

			//using (new TimedBlock($"{ typeof(Image).Name }.{ Utils.GetCurrentMethodName() }"))
			//{
			//	var bitmapImage = new BitmapImage(new Uri(Filename, UriKind.Absolute));

			//	OutputDimensionsWidth = bitmapImage.PixelWidth;
			//	OutputDimensionsHeight = bitmapImage.PixelHeight;
			//	base.Execute();

			//	var pixels = new byte[bitmapImage.PixelWidth * bitmapImage.PixelHeight * 4];
			//	bitmapImage.CopyPixels(pixels, bitmapImage.PixelWidth * 4, 0);

			//	for (var i = 0; i < Output.Length; i++)
			//	{
			//		Output[i].X = (double)pixels[i * 4 + 0] / 255.0;
			//		Output[i].Y = (double)pixels[i * 4 + 1] / 255.0;
			//		Output[i].Z = (double)pixels[i * 4 + 2] / 255.0;
			//		Output[i].W = (double)pixels[i * 4 + 3] / 255.0;
			//	}
			//}
		}
	}
}
