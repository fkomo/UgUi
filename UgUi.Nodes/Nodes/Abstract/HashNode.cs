
namespace Ujeby.UgUi.Nodes.Abstract
{
	[NodeInfo(Abstract = true)]
	public class HashNode : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, DisplayName = "Bin", AnchorOnly = true)]
		public byte[] Input { get; set; }

		[Output(AnchorOnly = true, DisplayName = "Hash")]
		public byte[] Output { get; set; }

		[Output(AnchorOnly = true)]
		public int Length { get { return Output?.Length ?? 0; } }

		public override string[] GetInputs()
		{
			return new string[]
			{
				//Input,
				$"{ nameof(Input) }.{ nameof(Length) }:{ (Input?.Length ?? 0).ToString() }",
			};
		}

		public override string[] GetOutputs()
		{
			return new string[]
			{
				//Output,
				$"{ nameof(Output) }.{ nameof(Length) }:{ Length.ToString() }",
			};
		}
	}
}
