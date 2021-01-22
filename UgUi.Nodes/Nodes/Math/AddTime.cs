using System;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Math
{
	[NodeInfo]
	public class AddTime : BinaryOperatorEx<DateTime, TimeSpan, DateTime>
	{
		public override void Execute()
		{
			if (A != null && B != null)
				C = A.Add(B);
			else
				C = A;
		}
	}
}
