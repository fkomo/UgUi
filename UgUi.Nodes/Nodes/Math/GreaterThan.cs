using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Math
{
	[NodeInfo]
	[IgnoredProperty(Name = nameof(BinaryOperator<double>.C))]
	public class GreaterThan : BinaryOperator<double>
	{
		[Output(AnchorOnly = true, DisplayName = "")]
		public bool Output { get; protected set; } = false;

		public override void Execute()
		{
			Output = A > B;
		}
	}
}
