using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Math
{
	[NodeInfo]
	public class Tan : UnaryOperator<v4>
	{
		public override void Execute()
		{
			if (input == null)
				output = new v4();
			else
				output = new v4(
					System.Math.Tan(input.X), 
					System.Math.Tan(input.Y), 
					System.Math.Tan(input.Z), 
					System.Math.Tan(input.W));
		}
	}
}
