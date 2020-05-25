using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Operations.Abstract;
using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Math
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
