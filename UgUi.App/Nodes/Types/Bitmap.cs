using System;
using System.IO;
using System.Windows.Media.Imaging;
using Ujeby.Common.Tools;
using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Active
{
	//[FunctionInfo]
	//public class Image : Generator
	//{
	//	private string filename = null;
	//	public string Filename
	//	{
	//		get { return filename; }
	//		set { filename = value; OnPropertyChanged(nameof(ImageName)); }
	//	}

	//	// TODO InputAttribute with file open dialog, also need to specify callback for dialog open
	//	//[Input(DisplayName = nameof(Filename), InputAnchor = true, ReadOnly = true, InputAnchorProperty = nameof(Filename))]
	//	public string ImageName { get { return string.IsNullOrEmpty(Filename) ? null : new FileInfo(Filename).Name; } }

	//	public override void Execute()
	//	{
	//		using (new TimedBlock($"{ typeof(Image).Name }.{ Utils.GetCurrentMethodName() }"))
	//		{
	//			var bitmapImage = new BitmapImage(new Uri(Filename, UriKind.Absolute));

	//			OutputDimensionsWidth = bitmapImage.PixelWidth;
	//			OutputDimensionsHeight = bitmapImage.PixelHeight;
	//			base.Execute();

	//			var pixels = new byte[bitmapImage.PixelWidth * bitmapImage.PixelHeight * 4];
	//			bitmapImage.CopyPixels(pixels, bitmapImage.PixelWidth * 4, 0);

	//			for (var i = 0; i < Output.Length; i++)
	//			{
	//				Output[i].X = (double)pixels[i * 4 + 0] / 255.0;
	//				Output[i].Y = (double)pixels[i * 4 + 1] / 255.0;
	//				Output[i].Z = (double)pixels[i * 4 + 2] / 255.0;
	//				Output[i].W = (double)pixels[i * 4 + 3] / 255.0;
	//			}
	//		}
	//	}
	//}
}
