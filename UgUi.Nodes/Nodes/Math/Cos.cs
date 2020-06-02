using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Math
{
	[NodeInfo]
	public class Cos : UnaryOperator<v4>
	{
		public override void Execute()
		{
			if (input == null)
				output = new v4();
			else
				output = new v4(
					System.Math.Cos(input.X), 
					System.Math.Cos(input.Y), 
					System.Math.Cos(input.Z), 
					System.Math.Cos(input.W));
		}
	}
}
