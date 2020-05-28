using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Ujeby.Common.Tools;
using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Nodes;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.IO
{
	[NodeInfo]
	[IgnoredProperty(Name = nameof(Read))]
	public class ImageFile : _File<v4[]>
	{
		[Output(AnchorOnly = true)]
		public v2 Size { get; protected set; }

		[Output(AnchorOnly = true)]
		public v4[] BGRA { get; protected set; }

		[Output(AnchorOnly = true)]
		public byte[] Raw { get; protected set; }

		public override void Execute()
		{
			try
			{
				Length = 0;
				Size = new v2();
				Raw = new byte[] { };
				BGRA = new v4[] { };

				if (!string.IsNullOrEmpty(Path) && File.Exists(Path))
				{
					// TODO NODE ImageFile.Write

					// read
					var bitmapImage = new BitmapImage(new Uri(Path, UriKind.Absolute));
					Size = new v2(bitmapImage.PixelWidth, bitmapImage.PixelHeight);

					Raw = new byte[bitmapImage.PixelWidth * bitmapImage.PixelHeight * 4];
					bitmapImage.CopyPixels(Raw, bitmapImage.PixelWidth * 4, 0);

					BGRA = new v4[bitmapImage.PixelWidth * bitmapImage.PixelHeight];
					for (var i = 0; i < BGRA.Length; i++)
						BGRA[i] = new v4(
							(double)Raw[i * 4 + 0] / 255.0,
							(double)Raw[i * 4 + 1] / 255.0,
							(double)Raw[i * 4 + 2] / 255.0,
							(double)Raw[i * 4 + 3] / 255.0);
				}

				Length = Raw?.Length ?? 0;
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		public override string[] GetOutputs()
		{
			return base.GetOutputs().Concat(
				new string[]
				{
					$"{ nameof(Size) }:{ Size.ToString() }",
				}).ToArray();
		}
	}
}
