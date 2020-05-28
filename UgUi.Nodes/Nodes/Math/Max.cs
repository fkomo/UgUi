using System.Globalization;
using Ujeby.UgUi.Nodes;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Math
{
	[NodeInfo]
	public class Max : BinaryOperator<double>
	{
		public override void Execute()
		{
			C = System.Math.Max(A, B);
		}

		public override string[] GetOutputs()
		{
			return new string[] { C.ToString("F4", CultureInfo.InvariantCulture) };
		}
	}
}
