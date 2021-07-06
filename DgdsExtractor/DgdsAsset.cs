using System;
using System.Collections.Generic;
using System.IO;

namespace DgdsExtractor
{
	class DgdsAsset
	{
		private string filename;
		private bool isFlatFile;
		private int nameHash;
		private uint offset;
		private byte[] data;
		private List<DgdsChunk> chunks;

		public DgdsAsset(int nameHash, uint offset)
		{
			this.nameHash = nameHash;
			this.offset = offset;
		}

		// Seek to asset location in volume file and extract asset data
		public void ReadAsset(BinaryReader file)
		{
			file.BaseStream.Seek(offset, SeekOrigin.Begin);
			this.filename = DgdsUtilities.ReadFilename(file);
			this.isFlatFile = DgdsMetadata.IsFlatFile(DgdsMetadata.GetAssetType(filename.Substring(filename.LastIndexOf('.') + 1)));

			uint size = file.ReadUInt32();
			this.data = file.ReadBytes(Convert.ToInt32(size));

			if (!isFlatFile)
			{
				chunks = new List<DgdsChunk>();
				using BinaryReader dataReader = new BinaryReader(new MemoryStream(this.data));
				ReadChunks(dataReader, AssetType.NONE);
			}
		}

		// Recursively read all chunks contained in this asset's data
		private void ReadChunks(BinaryReader data, AssetType type)
		{
			while (data.BaseStream.Position + 1 < data.BaseStream.Length)
			{
				DgdsChunk chunk = new DgdsChunk(type);
				chunk.ReadChunk(data);
				if (chunk.IsContainer)
				{
					ReadChunks(data, DgdsMetadata.GetAssetType(chunk.Identifier));
				}
				else
				{
					chunks.Add(chunk);
				}
			}
		}

		// Write the asset's data to disk
		public void Write(string path)
		{
			using BinaryWriter writer = new BinaryWriter(File.Create(path + filename));
			if (isFlatFile)
			{
				writer.Write(data);
			}
			else
			{
				foreach (DgdsChunk chunk in chunks)
				{
					chunk.Write(writer);
				}
			}
		}

		// Print info about this asset to console
		public void PrintAsset()
		{
			Console.WriteLine("{0} ({1} bytes)", filename, data.Length);
			if (!isFlatFile)
			{
				foreach (DgdsChunk chunk in chunks)
				{
					chunk.PrintChunk();
				}
			}
		}
	}
}
