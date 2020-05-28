using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Nodes.Strings
{
	[NodeInfo]
	public class LastIndexOf : NodeBase
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

		protected int value = -1;
		[Output(ReadOnly = true, DisplayName = "")]
		public int Value
		{
			get { return value; }
			set { SetField(ref this.value, value, nameof(Value)); }
		}

		public override void Execute()
		{
			if (!string.IsNullOrEmpty(toFind) && !string.IsNullOrEmpty(Input))
				Value = Input.ToLower().LastIndexOf(toFind.ToLower());
			else
				Value = -1;
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
			return new string[] { Value.ToString() };
		}
	}
}
