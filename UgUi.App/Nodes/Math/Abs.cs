using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Operations.Abstract;
using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Math
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
