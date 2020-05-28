using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Nodes.Strings
{
	[NodeInfo]
	public class Join : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true)]
		public string[] Array { get; set; }

		private string separator;
		[Input(Order = 1, InputAnchor = true, Serializable = true)]
		public string Separator
		{
			get { return separator; }
			set { SetField(ref separator, value, nameof(Separator)); }
		}

		[Output(AnchorOnly = true, DisplayName = "")]
		public string Output { get; private set; }

		[Output(AnchorOnly = true)]
		public int Length { get; private set; }

		public override void Execute()
		{
			if (Array != null)
				Output = string.Join(Separator ?? string.Empty, Array);
			else
				Output = string.Empty;

			Length = Output.Length;
		}

		public override string[] GetInputs()
		{
			return new string[] 
			{
				$"{ nameof(Array) }.{ nameof(Array.Length) }:{ (Array != null ? Array.Length : 0).ToString() }",
				$"{ nameof(Separator) }:{ Separator }",
			};
		}

		public override string[] GetOutputs()
		{
			return new string[]
			{
				Output,
				$"{ nameof(Length) }:{ Length.ToString() }",
			};
		}
	}
}
