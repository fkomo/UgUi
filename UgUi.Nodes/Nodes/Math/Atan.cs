using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Math
{
	[NodeInfo]
	public class Atan : UnaryOperator<v4>
	{
		public override void Execute()
		{
			if (input == null)
				output = new v4();
			else
				output = new v4(
					System.Math.Atan(input.X), 
					System.Math.Atan(input.Y), 
					System.Math.Atan(input.Z), 
					System.Math.Atan(input.W));
		}
	}
}
