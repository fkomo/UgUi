using System;

namespace Ujeby.UgUi.Nodes.Generators
{
	[NodeInfo(DisplayName = "Guid")]
	public class _Guid : NodeBase
	{
		protected string value = Guid.Empty.ToString();
		[Output(AnchorOnly = false, ReadOnly = true, DisplayName = "")]
		public string Value
		{
			get { return value; }
			set 
			{
				SetField(ref this.value, value, nameof(Value)); 
			}
		}

		public override void Execute()
		{
			Value = Guid.NewGuid().ToString();
		}

		public override string[] GetOutputs()
		{
			return new string[] { Value };
		}
	}
}
