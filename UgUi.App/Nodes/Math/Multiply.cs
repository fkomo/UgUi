using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Operations.Abstract;
using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Math
{
	[NodeInfo]
	public class Multiply : BinaryOperator<v4>
	{
		public override void Execute()
		{
			if (A != null && B != null)
				C = new v4(A.X * B.X, A.Y * B.Y, A.Z * B.Z, A.W * B.W);
			else
				C = new v4();
		}
	}
}
