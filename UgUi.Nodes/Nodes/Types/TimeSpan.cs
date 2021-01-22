using System;

namespace Ujeby.UgUi.Nodes.Types
{
	[NodeInfo(DisplayName = "TimeSpan")]
	public class _TimeSpan : NodeBase
	{
		protected int h = 0;
		[Input(Order = 1, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public int Hh
		{
			get { return h; }
			set { SetField(ref h, value, nameof(Hh)); }
		}

		protected int m = 0;
		[Input(Order = 2, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public int Mm
		{
			get { return m; }
			set { SetField(ref m, value, nameof(Mm)); }
		}

		protected int s = 0;
		[Input(Order = 3, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public int Ss
		{
			get { return s; }
			set { SetField(ref s, value, nameof(Ss)); }
		}

		[Input(Order = 4, InputAnchor = true, AnchorOnly = true, OutputAnchor = true)]
		public TimeSpan T
		{
			get { return new TimeSpan(Hh, Mm, Ss); }
			set
			{
				Hh = (int)value.Hours;
				Mm = (int)value.Minutes;
				Ss = (int)value.Seconds;
			}
		}

		public override void Execute()
		{

		}

		public override string[] GetOutputs()
		{
			return new string[]
			{
				T.ToString(),
			};
		}
	}
}
