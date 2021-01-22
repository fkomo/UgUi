
namespace Ujeby.UgUi.Nodes.Abstract
{
	[NodeInfo(Abstract = true)]
	public class BinaryOperatorEx<TIn1, TIn2, TOut> : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true)]
		public TIn1 A { get; set; }

		[Input(Order = 1, InputAnchor = true, AnchorOnly = true)]
		public TIn2 B { get; set; }

		[Output(AnchorOnly = true)]
		public TOut C { get; protected set; }

		public override string[] GetInputs()
		{
			return new string[]
			{
				A?.ToString(),
				B?.ToString(),
			};
		}

		public override string[] GetOutputs()
		{
			return new string[] { C?.ToString() };
		}
	}
}
