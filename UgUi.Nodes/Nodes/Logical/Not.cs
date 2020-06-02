using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Logical
{
	[NodeInfo]
	public class NOT : UnaryOperator<bool>
	{
		public override void Execute()
		{
			output = !input;
		}
	}
}
