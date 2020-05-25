using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Types
{
	[NodeInfo(DisplayName = "String")]
	public class _String : NodeOperationBase
	{
		protected string input = string.Empty;

		protected string value = string.Empty;
		[Input(Order = 0, InputAnchor = true, OutputAnchor = true, DisplayName = "", Serializable = true)]
		public string Value
		{
			get { return value; }
			set 
			{ 
				input = value; 
				SetField(ref this.value, value, nameof(Value)); 
			}
		}

		[Output(AnchorOnly = true)]
		public int Length { get; private set; } = 0;

		public override void Execute()
		{
			Length = (Value ?? string.Empty).Length;
		}

		public override string[] GetInputs()
		{
			return new string[]
			{
				input,
			};
		}

		public override string[] GetOutputs()
		{
			return new string[]
			{
				Value,
				$"{ nameof(Length) }:{ Length.ToString() }",
			};
		}
	}
}
