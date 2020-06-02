
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Strings
{
	[NodeInfo]
	[IgnoredProperty(Name = nameof(BinaryOperator<string>.C))]
	public class Compare : BinaryOperator<string>
	{
		[Output(AnchorOnly = true, DisplayName = "")]
		public bool Output { get; protected set; } = false;

		public override void Execute()
		{
			Output = string.Compare(A, B) == 0;
		}

		public override string[] GetInputs()
		{
			return new string[] 
			{ 
				A,
				B,
			};
		}

		public override string[] GetOutputs()
		{
			return new string[]
			{
				Output.ToString()
			};
		}
	}
}
