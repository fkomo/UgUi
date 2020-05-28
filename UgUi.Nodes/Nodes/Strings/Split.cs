using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Nodes.Strings
{
	[NodeInfo]
	public class Split : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true, DisplayName = "")]
		public string Input { get; set; }

		private string separator;
		[Input(Order = 1, InputAnchor = true, Serializable = true)]
		public string Separator
		{
			get { return separator; }
			set { SetField(ref separator, value, nameof(Separator)); }
		}

		[Output(AnchorOnly = true, DisplayName = "")]
		public string[] Output { get; private set; }

		[Output(AnchorOnly = true)]
		public int Count { get; private set; } = 0;

		public override void Execute()
		{
			if (!string.IsNullOrEmpty(Separator) && !string.IsNullOrEmpty(Input))
				Output = Input.Split(new string[] { Separator }, System.StringSplitOptions.RemoveEmptyEntries);
			else
				Output = new string[] { };

			Count = Output?.Length ?? 0;
		}

		public override string[] GetInputs()
		{
			return new string[] 
			{ 
				Input,
				$"{ nameof(Separator) }:{ Separator }",
			};
		}

		public override string[] GetOutputs()
		{
			return new string[]
			{
				$"{ nameof(Count) }:{ Count.ToString() }",
			};
		}
	}
}
