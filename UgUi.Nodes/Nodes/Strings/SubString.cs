using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Nodes.Strings
{
	[NodeInfo]
	public class SubString : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true, DisplayName = "")]
		public string Input { get; set; }

		private int start = 0;
		[Input(Order = 1, InputAnchor = true, Serializable = true)]
		public int Start
		{
			get { return start; }
			set { SetField(ref start, value, nameof(Start)); }
		}

		private int length = 0;
		[Input(Order = 2, InputAnchor = true, Serializable = true)]
		public int Length
		{
			get { return length; }
			set { SetField(ref length, value, nameof(Length)); }
		}

		[Output(AnchorOnly = true, DisplayName = "")]
		public string Output { get; private set; } = string.Empty;

		public override void Execute()
		{
			Output = (Input ?? string.Empty).Substring(start, System.Math.Min(length, (Input ?? string.Empty).Length - start));
		}

		public override string[] GetInputs()
		{
			return new string[] 
			{ 
				Input,
				$"{ nameof(Start) }:{ Start }",
				$"{ nameof(Length) }:{ Length }",
			};
		}

		public override string[] GetOutputs()
		{
			return new string[] { Output };
		}
	}
}
