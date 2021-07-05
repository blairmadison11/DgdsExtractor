using System;
using System.Collections.Generic;
using System.Text;
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

		public string Filename { get => filename; set => filename = value; }
		public uint Offset { get => offset; }
		public byte[] Data { get => data; set => data = value; }
		public int Size { get => (data == null) ? 0 : data.Length; }

		public DgdsAsset(int nameHash, uint offset)
		{
			this.nameHash = nameHash;
			this.offset = offset;
		}

		public void ReadAsset(BinaryReader file)
		{
			file.BaseStream.Seek(offset, SeekOrigin.Begin);
			this.filename = DgdsUtilities.ReadString(file, 13);
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

		private void ReadChunks(BinaryReader data, AssetType type)
		{
			while (data.BaseStream.Position + 1 < data.BaseStream.Length)
			{
				DgdsChunk chunk = new DgdsChunk(type);
				chunk.ReadChunk(data);
				if (chunk.IsContainer)
				{
					ReadChunks(data, DgdsMetadata.GetAssetType(chunk.Extension));
				}
				else
				{
					chunks.Add(chunk);
				}
			}
		}

		public void Write(string path)
		{
			using (BinaryWriter writer = new BinaryWriter(File.Create(path + filename)))
			{
				if (isFlatFile)
				{
					writer.Write(data);
				}
				else
				{
					for (int i = 0; i < chunks.Count; ++i)
					{
						chunks[i].Write(writer);
					}
				}
			}
		}

		public void PrintAsset()
		{
			Console.WriteLine("{0} ({1} bytes)", filename, Size);
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
