using System.Windows.Media;
using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Nodes.Types
{
	[NodeInfo(DisplayName = "Color")]
	public class _Color : NodeBase
	{
		private double a = 1.0;
		[Input(Order = 0, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public double A
		{
			get { return a; }
			set
			{
				SetField(ref a, value, nameof(A));
				OnPropertyChanged(nameof(Brush));
			}
		}

		private double r = 0.0;
		[Input(Order = 1, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public double R
		{
			get { return r; }
			set 
			{ 
				SetField(ref r, value, nameof(R));
				OnPropertyChanged(nameof(Brush));
			}
		}

		private double g = 0.0;
		[Input(Order = 2, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public double G
		{
			get { return g; }
			set 
			{
				SetField(ref g, value, nameof(G));
				OnPropertyChanged(nameof(Brush));
			}
		}

		private double b = 0.0;
		[Input(Order = 3, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public double B
		{
			get { return b; }
			set 
			{ 
				SetField(ref b, value, nameof(B));
				OnPropertyChanged(nameof(Brush));
			}
		}

		[Input(Order = 5, InputAnchor = true, OutputAnchor = true, DisplayName = "")]
		public SolidColorBrush Brush
		{
			get { return new SolidColorBrush(Color.FromArgb((byte)(A * 255), (byte)(R * 255), (byte)(G * 255), (byte)(B * 255))); }
			set 
			{
				if (value == null)
				{
					R = G = B = A = 0.0;
				}
				else
				{
					R = value.Color.R / 255.0;
					G = value.Color.G / 255.0;
					B = value.Color.B / 255.0;
					A = value.Color.A / 255.0;
				}
			}
		}

		public override string[] GetOutputs()
		{
			return new string[]
			{
				Brush.Color.ToString(),
			};
		}
	}
}
