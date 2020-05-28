using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Nodes.Types
{
	[NodeInfo]
	public class Boolean : NodeBase
	{
		private bool value = false;
		[Input(Order = 0, InputAnchor = true, DisplayName = "", OutputAnchor = true, Serializable = true)]
		public bool Value
		{
			get { return value; }
			set { SetField(ref this.value, value, nameof(Value)); }
		}

		public override string[] GetOutputs()
		{
			return new string[] { Value.ToString() };
		}
	}
}
