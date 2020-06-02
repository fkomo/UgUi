
namespace Ujeby.UgUi.Nodes.Abstract
{
	[NodeInfo(Abstract = true)]
	public class EncodingNode : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true, DisplayName = "Text")]
		public string InputText { get; set; }

		[Input(Order = 1, InputAnchor = true, AnchorOnly = true, DisplayName = "Bin")]
		public byte[] InputBin { get; set; }

		[Output(AnchorOnly = true, DisplayName = "Bin")]
		public byte[] OutputBin { get; set; }

		[Output(AnchorOnly = true, DisplayName = "Text")]
		public string OutputText { get; set; }

		public override string[] GetInputs()
		{
			return new string[]
			{
				//InputText,
				//InputBin,
				$"{ nameof(InputText) }.Length:{ (InputText?.Length ?? 0).ToString() }",
				$"{ nameof(InputBin) }.Length:{ (InputBin?.Length ?? 0).ToString() }",
			};
		}

		public override string[] GetOutputs()
		{
			return new string[]
			{
				//OutputBin,
				//OutputText,
				$"{ nameof(OutputText) }.Length:{ (OutputText?.Length ?? 0).ToString() }",
				$"{ nameof(OutputBin) }.Length:{ (OutputBin?.Length ?? 0).ToString() }",
			};
		}
	}
}
