using System.IO;
using System.IO.Compression;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Arrays
{
	[NodeInfo]
	public class Deflate : CompressNode
	{
		public override void Execute()
		{
			if (Input?.Length > 0)
			{
				using (var inputStream = new MemoryStream(Input))
				{
					using (var outputStream = new MemoryStream())
					{
						using (var compressed = new DeflateStream(outputStream, CompressionMode.Compress))
							CopyTo(inputStream, compressed);

						Output = outputStream.ToArray();
					}
				}
			}
			else
				Output = new byte[] { };
		}
	}
}