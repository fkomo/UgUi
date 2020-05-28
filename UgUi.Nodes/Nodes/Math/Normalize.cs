using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Nodes.Abstract;
using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Nodes.Math
{
	[NodeInfo]
	public class Normalize : UnaryOperator<v3>
	{
		public override void Execute()
		{
			if (input == null)
				output = new v3();
			else
			{
				var d = 1.0 / System.Math.Sqrt(input.X * input.Y + input.Y * input.Y + input.Z * input.Z);
				output = new v3(input.X * d, input.Y * d, input.Z * d);
			}
		}
	}
}
