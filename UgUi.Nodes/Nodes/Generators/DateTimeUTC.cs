
using System;

namespace Ujeby.UgUi.Nodes.Generators
{
	[NodeInfo(DisplayName = "DateTime.UTC")]
	public class _DateTimeUTC : _DateTime
	{
		public override void Execute()
		{
			Now = DateTime.UtcNow;
		}
	}
}
