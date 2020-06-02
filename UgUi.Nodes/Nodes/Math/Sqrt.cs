using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Math
{
	[NodeInfo]
	public class Sqrt : UnaryOperator<v4>
	{
		public override void Execute()
		{
			if (input == null)
				output = new v4();
			else
				output = new v4(
					System.Math.Sqrt(input.X), 
					System.Math.Sqrt(input.Y), 
					System.Math.Sqrt(input.Z), 
					System.Math.Sqrt(input.W));
		}
	}
}
