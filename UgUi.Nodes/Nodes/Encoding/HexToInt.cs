using System;

namespace Ujeby.UgUi.Nodes.Encoding
{
	[NodeInfo]
	public class HexToInteger : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true, DisplayName = "0x")]
		public string InputHex { get; set; }

		private int output = 0;
		[Output(DisplayName = "", ReadOnly = true)]
		public int Output
		{
			get { return output; }
			set { SetField(ref output, value, nameof(Output)); }
		}

		public override string[] GetInputs()
		{
			return new string[] { InputHex };
		}

		public override string[] GetOutputs()
		{
			return new string[] { Output.ToString() };
		}

		public override void Execute()
		{
			if (string.IsNullOrEmpty(InputHex))
				Output = 0;

			else
				Output = Convert.ToInt32(InputHex, 16);
		}
	}
}
