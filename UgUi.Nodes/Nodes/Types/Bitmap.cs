using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Nodes.Types
{
	[NodeInfo]
	public class Bitmap : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true)]
		public v4[] BGRA { get; set; }

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
		[ImageBindings(Width = "SizeX", Height = "SizeY")]
		public BitmapSource Output 
		{
			get { return output; }
			set { SetField(ref output, value, nameof(Output)); } 
		}

		public override void Execute()
		{
			if (BGRA != null && Size != null && Size.X * Size.Y == BGRA.Length)
			{
				var pixels = new byte[(int)Size.X * (int)Size.Y * 4];
				for (var i = 0; i < BGRA.Length; i++)
				{
					pixels[i * 4 + 0] = (byte)(BGRA[i].X * 255);
					pixels[i * 4 + 1] = (byte)(BGRA[i].Y * 255);
					pixels[i * 4 + 2] = (byte)(BGRA[i].Z * 255);
					pixels[i * 4 + 3] = (byte)(BGRA[i].W * 255);
				}

				Output = BitmapSource.Create((int)Size.X, (int)Size.Y, 96, 96, PixelFormats.Bgra32, null, pixels, (int)Size.X * 4);
			}
			else
				Output = null;
		}

		public override string[] GetInputs()
		{
			return new string[]
			{
				$"{ nameof(Size) }:{ Size?.ToString() }" ,
			};
		}
	}
}
