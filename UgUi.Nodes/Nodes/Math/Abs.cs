using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Nodes.Abstract;
using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Nodes.Math
{
	[NodeInfo]
	public class Abs : UnaryOperator<v4>
	{
		public override void Execute()
		{
			if (input == null)
				output = new v4();
			else
				output = new v4(System.Math.Abs(input.X), System.Math.Abs(input.Y), System.Math.Abs(input.Z), System.Math.Abs(input.W));
		}
	}
}
