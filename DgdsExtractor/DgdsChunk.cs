using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace DgdsExtractor
{
	class DgdsChunk
	{
		private string extension;
		private AssetType type = AssetType.NONE;
		private AssetSection section = AssetSection.NONE;
		private byte[] chunkData;
		private bool isContainer, isPacked;

		public bool IsContainer { get => isContainer; }
		public string Extension { get => extension; }

		public DgdsChunk(AssetType type)
		{
			this.type = type;
		}

		public void ReadChunk(BinaryReader data)
		{
			byte[] ext = data.ReadBytes(4);
			if (ext[3] != ':')
			{
				throw new Exception("Invalid header!");
			}

			if (ext[2] == 0)
			{
				extension = string.Join("", Convert.ToChar(ext[0]), Convert.ToChar(ext[1]));
			}
			else
			{
				extension = string.Join("",
					Convert.ToChar(ext[0]),
					Convert.ToChar(ext[1]),
					Convert.ToChar(ext[2]));
			}

			if (type == AssetType.NONE)
			{
				type = DgdsMetadata.GetAssetType(extension);
			}
			else
			{
				section = DgdsMetadata.GetAssetSection(extension);
			}
			isPacked = DgdsMetadata.IsPacked(type, section);

			uint sizeData = data.ReadUInt32();
			this.isContainer = (sizeData >> 31) == 1;
			uint size = (sizeData & 0x7FFFFFFF);
			
			if (!isContainer)
			{
				if (isPacked)
				{
					size -= 5;
					byte compressionType = data.ReadByte();
					data.ReadUInt32();

					byte[] compressedData = data.ReadBytes(Convert.ToInt32(size));

					this.chunkData = DgdsUtilities.Decompress(compressionType, compressedData);
				}
				else
				{
					this.chunkData = data.ReadBytes(Convert.ToInt32(size));
				}
			}
		}

		public void Write(BinaryWriter writer)
		{
			if (chunkData != null)
			{
				writer.Write(chunkData);
			}
		}

		public void PrintChunk()
		{
			if (chunkData != null)
			{
				Console.WriteLine("\tChunk: {0} ({1} bytes)", extension, chunkData.Length);
			}
			else
			{
				Console.WriteLine("\tChunk: {0}", extension);
			}
		}
	}
}
