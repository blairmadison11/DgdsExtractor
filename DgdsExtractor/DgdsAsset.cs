using System;
using System.Collections.Generic;
using System.IO;

namespace DgdsExtractor
{
	class DgdsAsset
	{
		private string filename;
		private AssetType assetType;
		private bool isFlatFile;
		private readonly uint offset;
		private byte[] data;
		private List<DgdsChunk> chunks;

		public DgdsAsset(uint offset)
		{
			this.offset = offset;
		}

		// Seek to asset location in volume file and extract asset data
		public void ReadAsset(BinaryReader inFile)
		{
			inFile.BaseStream.Seek(offset, SeekOrigin.Begin);
			filename = DgdsUtilities.ReadFilename(inFile);
			assetType = DgdsMetadata.GetAssetType(filename[(filename.LastIndexOf('.') + 1)..]);
			isFlatFile = DgdsMetadata.IsFlatFile(assetType);

			int size = Convert.ToInt32(inFile.ReadUInt32());
			data = inFile.ReadBytes(size);

			if (!isFlatFile)
			{
				chunks = new List<DgdsChunk>();
				using BinaryReader assetData = new BinaryReader(new MemoryStream(data));
				ReadChunks(assetData, assetType);
			}
		}

		// Recursively read all chunks contained in this asset's data
		private void ReadChunks(BinaryReader assetData, AssetType type)
		{
			while (assetData.BaseStream.Position + 1 < assetData.BaseStream.Length)
			{
				DgdsChunk chunk = new DgdsChunk(type);
				chunk.ReadChunk(assetData);
				if (chunk.IsContainer)
				{
					ReadChunks(assetData, chunk.ChunkType);
				}
				else
				{
					chunks.Add(chunk);
				}
			}
		}

		// Write the asset's data to disk
		public void WriteData(string path)
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
					chunk.WriteData(writer);
				}
			}
		}

		// Write all text lines (dialogue, descriptions, etc.) contained in this asset to disk
		public void WriteText(StreamWriter outFile)
		{
			if (!isFlatFile && assetType == AssetType.SDS)
			{
				outFile.Write("******************** {0} ********************\n\n", this.filename);
				foreach (DgdsChunk chunk in chunks)
				{
					chunk.WriteText(outFile);
				}
			}
		}

		// Write log information to output stream
		public void WriteLog(StreamWriter output)
		{
			output.WriteLine("{0} ({1} bytes)", filename, data.Length);
			if (!isFlatFile)
			{
				foreach (DgdsChunk chunk in chunks)
				{
					chunk.WriteLog(output);
				}
			}
		}
	}
}
