using System;

namespace Ujeby.UgUi.Nodes.Arrays
{
	[NodeInfo]
	public class AtIndex : NodeBase
	{
		protected Array array;
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true)]
		public Array Array { get; set; }

		protected int index = 0;
		[Input(Order = 1, InputAnchor = true, Serializable = true)]
		public int Index
		{
			get { return index; }
			set { SetField(ref index, value, nameof(Index)); }
		}

		protected object value;
		[Output(AnchorOnly = true)]
		public object Value
		{
			get { return value; }
			set { SetField(ref this.value, value, nameof(Value)); }
		}

		public override void Execute()
		{
			if (Array != null && Index < Array.Length)
				Value = Array.GetValue(Index);
			else
				Value = null;
		}

		public override string[] GetInputs()
		{
			return new string[]
			{
				Index.ToString(),
			};
		}

		public override string[] GetOutputs()
		{
			return new string[] 
			{ 
				Value?.ToString()
			};
		}
	}
}
