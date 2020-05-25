using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Strings
{
	[NodeInfo]
	public class Contains : NodeOperationBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true, DisplayName = "")]
		public string Input { get; set; }

		protected string toFind;
		[Input(Order = 1, InputAnchor = true, Serializable = true)]
		public string ToFind
		{
			get { return toFind; }
			set { SetField(ref toFind, value, nameof(ToFind)); }
		}

		[Output(AnchorOnly = true, DisplayName = "")]
		public bool Output { get; private set; } = false;

		public override void Execute()
		{
			if (toFind != null)
				Output = (Input ?? string.Empty).Contains(toFind);
			else
				Output = false;
		}

		public override string[] GetInputs()
		{
			return new string[] 
			{ 
				Input,
				$"{ nameof(ToFind) }:{ ToFind }",
			};
		}

		public override string[] GetOutputs()
		{
			return new string[] { Output.ToString() };
		}
	}
}
