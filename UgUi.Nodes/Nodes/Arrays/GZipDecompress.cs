using System.IO;
using System.IO.Compression;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Arrays
{
	[NodeInfo]
	public class GZipDecompress : CompressNode
	{
		public override void Execute()
		{
			if (Input?.Length > 0)
			{
				using (var inputStream = new MemoryStream(Input))
				{
					using (var outputStream = new MemoryStream())
					{
						using (var decompressed = new GZipStream(inputStream, CompressionMode.Decompress))
							CopyTo(decompressed, outputStream);

						Output = outputStream.ToArray();
					}
				}
			}
			else
				Output = new byte[] { };
		}
	}
}