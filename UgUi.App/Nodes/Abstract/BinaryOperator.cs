using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Abstract
{
	[NodeInfo(Abstract = true)]
	public class BinaryOperator<T> : NodeOperationBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true)]
		public T A { get; set; }

		[Input(Order = 1, InputAnchor = true, AnchorOnly = true)]
		public T B { get; set; }

		[Output(AnchorOnly = true)]
		public T C { get; protected set; }

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
