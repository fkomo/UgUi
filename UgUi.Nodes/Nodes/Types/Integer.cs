
namespace Ujeby.UgUi.Nodes.Types
{
	[NodeInfo]
	public class Integer : NodeBase
	{
		private int x = 0;
		[Input(Order = 0, InputAnchor = true, DisplayName = "", OutputAnchor = true, Serializable = true)]
		public int X
		{
			get { return x; }
			set { SetField(ref x, value, nameof(X)); }
		}

		public override string[] GetOutputs()
		{
			return new string[] { X.ToString() };
		}
	}
}
