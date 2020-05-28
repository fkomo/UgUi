using System.Globalization;
using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Nodes;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Math
{
	[NodeInfo]
	[IgnoredProperty(Name = nameof(BinaryOperator<v4>.C))]
	public class Dot : BinaryOperator<v4>
	{
		[Output(AnchorOnly = true, DisplayName = "")]
		public double DotProduct { get; private set; } = 0.0;

		public override void Execute()
		{
			if (A != null && B != null)
				DotProduct = A.X * B.X + A.Y * B.Y + A.Z * B.Z + A.W * B.W;
			else
				DotProduct = 0.0;
		}

		public override string[] GetOutputs()
		{
			return new string[] { DotProduct.ToString("F4", CultureInfo.InvariantCulture) };
		}
	}
}
