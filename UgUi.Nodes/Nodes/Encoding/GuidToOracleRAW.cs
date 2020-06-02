using System;

namespace Ujeby.UgUi.Nodes.Encoding
{
	[NodeInfo]
	public class GuidToOracleRAW : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true, DisplayName = "Guid")]
		public string Input { get; set; }

		protected string output = string.Empty;
		[Output(AnchorOnly = false, DisplayName = "", ReadOnly = true)]
		public string Output
		{
			get { return output; }
			set { SetField(ref output, value, nameof(Output)); }
		}

		public override void Execute()
		{
			if (Guid.TryParse(Input, out Guid guid))
			{
				var binaryData = guid.ToByteArray();
				Output = BitConverter.ToString(binaryData).Replace("-", string.Empty);
			}
			else
				Output = string.Empty;
		}

		public override string[] GetInputs()
		{
			return new string[]
			{
				Input
			};
		}

		public override string[] GetOutputs()
		{
			return new string[]
			{
				Output
			};
		}
	}
}
