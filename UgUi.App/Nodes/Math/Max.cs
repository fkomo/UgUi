using System.Globalization;
using Ujeby.UgUi.Operations.Abstract;
using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Math
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
