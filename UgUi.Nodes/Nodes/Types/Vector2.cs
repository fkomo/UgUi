using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Nodes.Types
{
	[NodeInfo]
	public class Vector2 : NodeBase
	{
		protected double x = 0.0;
		[Input(Order = 1, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public double X
		{
			get { return x; }
			set { SetField(ref x, value, nameof(X)); }
		}

		protected double y = 0.0;
		[Input(Order = 2, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public double Y
		{
			get { return y; }
			set { SetField(ref y, value, nameof(Y)); }
		}

		[Input(Order = 3, InputAnchor = true, AnchorOnly = true, OutputAnchor = true)]
		public v2 XY
		{
			get { return new v2(x, y); }
			set
			{
				X = value?.X ?? 0.0;
				Y = value?.Y ?? 0.0;
			}
		}

		[Output(AnchorOnly = true)]
		public double Length { get; protected set; }

		public override void Execute()
		{
			Length = System.Math.Sqrt(X * X + Y * Y);
		}

		public override string[] GetOutputs()
		{
			return new string[]
			{
				XY.ToString(),
				$"{ nameof(Length) }:{ Length }",
			};
		}
	}
}
