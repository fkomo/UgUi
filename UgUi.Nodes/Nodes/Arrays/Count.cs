using System;

namespace Ujeby.UgUi.Nodes.Arrays
{
	[NodeInfo]
	public class Count : NodeBase
	{
		protected Array array;
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true, DisplayName = "")]
		public Array Array
		{
			get { return array; }
			set { SetField(ref array, value, nameof(Array)); }
		}

		protected int value;
		[Output(ReadOnly = true, DisplayName = "")]
		public int Value
		{
			get { return value; }
			set { SetField(ref this.value, value, nameof(Value)); }
		}

		public override void Execute()
		{
			Value = Array?.Length ?? 0;
		}

		public override string[] GetOutputs()
		{
			return new string[] 
			{ 
				Value.ToString() 
			};
		}
	}
}
