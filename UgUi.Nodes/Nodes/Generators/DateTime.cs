
using System;

namespace Ujeby.UgUi.Nodes.Generators
{
	[NodeInfo(DisplayName = "DateTime")]
	public class _DateTime : NodeBase
	{
		protected DateTime now;
		[Output(AnchorOnly = false, ReadOnly = true, DisplayName = "")]
		public DateTime Now
		{
			get { return now; }
			set { SetField(ref this.now, value, nameof(Now)); }
		}

		public override void Execute()
		{
			Now = System.DateTime.Now;
		}

		public override string[] GetOutputs()
		{
			return new string[] { Now.ToString() };
		}
	}
}
