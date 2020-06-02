namespace Ujeby.UgUi.Nodes.Abstract
{
	[NodeInfo(Abstract = true)]
	public class UnaryOperator<T> : NodeBase
	{
		protected T input;
		protected T output;
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true, OutputAnchor = true, DisplayName = "")]
		public T Output
		{ 
			get { return output; } 
			set { input = value; } 
		}

		public override string[] GetInputs()
		{
			return new string[] { input?.ToString() };
		}

		public override string[] GetOutputs()
		{
			return new string[] { Output?.ToString() };
		}
	}
}
