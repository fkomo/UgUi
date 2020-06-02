using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Math
{
	[NodeInfo]
	public class Sin : UnaryOperator<v4>
	{
		public override void Execute()
		{
			if (input == null)
				output = new v4();
			else
				output = new v4(
					System.Math.Sin(input.X), 
					System.Math.Sin(input.Y), 
					System.Math.Sin(input.Z), 
					System.Math.Sin(input.W));
		}
	}
}
