using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Nodes.Strings
{
	[NodeInfo]
	public class Replace : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true, DisplayName = "")]
		public string Input { get; set; }

		protected string old;
		[Input(Order = 1, InputAnchor = true, Serializable = true)]
		public string Old
		{
			get { return old; }
			set { SetField(ref old, value, nameof(Old)); }
		}

		protected string _new;
		[Input(Order = 2, InputAnchor = true, Serializable = true)]
		public string New
		{
			get { return _new; }
			set { SetField(ref _new, value, nameof(New)); }
		}

		[Output(AnchorOnly = true, DisplayName = "")]
		public string Output { get; private set; } = string.Empty;

		public override void Execute()
		{
			if (old != null)
				Output = (Input ?? string.Empty).Replace(old, _new);
			else
				Output = Input;
		}

		public override string[] GetInputs()
		{
			return new string[] 
			{ 
				Input,
				$"{ nameof(Old) }:{ Old }",
				$"{ nameof(New) }:{ New }",
			};
		}

		public override string[] GetOutputs()
		{
			return new string[] { Output.ToString() };
		}
	}
}
