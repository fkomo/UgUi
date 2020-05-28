using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Nodes.Types
{
	[NodeInfo]
	[IgnoredProperty(Name = nameof(Vector2.XY))]
	public class Vector3 : Vector2
	{
		protected double z = 0.0;
		[Input(Order = 3, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public double Z
		{
			get { return z; }
			set { SetField(ref z, value, nameof(Z)); }
		}

		[Input(Order = 4, InputAnchor = true, AnchorOnly = true, OutputAnchor = true)]
		public v3 XYZ
		{
			get { return new v3(x, y, z); }
			set
			{
				X = value?.X ?? 0.0;
				Y = value?.Y ?? 0.0;
				Z = value?.Z ?? 0.0;
			}
		}

		public override void Execute()
		{
			Length = System.Math.Sqrt(X * X + Y * Y + Z * Z);
		}

		public override string[] GetOutputs()
		{
			return new string[]
			{
				XYZ.ToString(),
				$"{ nameof(Length) }:{ Length }",
			};
		}
	}
}
