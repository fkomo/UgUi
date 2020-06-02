using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Logical
{
	[NodeInfo]
	public class AND : BinaryOperator<bool>
	{
		public override void Execute()
		{
			C = A && B;
		}
	}
}
