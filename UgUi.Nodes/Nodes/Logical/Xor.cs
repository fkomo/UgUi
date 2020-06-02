using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Logical
{
	[NodeInfo]
	public class XOR : BinaryOperator<bool>
	{
		public override void Execute()
		{
			C = A ^ B;
		}
	}
}
