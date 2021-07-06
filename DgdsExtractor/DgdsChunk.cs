using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace DgdsExtractor
{
	class DgdsChunk
	{
		private string identifier;
		private AssetType type = AssetType.NONE;
		private AssetSection section = AssetSection.NONE;
		private byte[] chunkData;
		private bool isContainer;

		public bool IsContainer { get => isContainer; }
		public string Identifier { get => identifier; }

		public DgdsChunk(AssetType type)
		{
			this.type = type;
		}

		// Parses the chunk data from the specified asset data
		public void ReadChunk(BinaryReader data)
		{
			byte[] ext = data.ReadBytes(4);
			if (ext[3] != ':')
			{
				throw new Exception("Invalid header!");
			}

			if (ext[2] == 0)
			{
				identifier = string.Concat(Convert.ToChar(ext[0]), Convert.ToChar(ext[1]));
			}
			else
			{
				identifier = string.Concat(Convert.ToChar(ext[0]), Convert.ToChar(ext[1]), Convert.ToChar(ext[2]));
			}

			if (type == AssetType.NONE)
			{
				type = DgdsMetadata.GetAssetType(identifier);
			}
			else
			{
				section = DgdsMetadata.GetAssetSection(identifier);
			}

			uint sizeData = data.ReadUInt32();
			this.isContainer = (sizeData >> 31) == 1;
			int size = Convert.ToInt32(sizeData & 0x7FFFFFFF);
			
			if (!isContainer)
			{
				if (DgdsMetadata.IsCompressed(type, section))
				{
					byte compressionType = data.ReadByte();
					uint unpackSize = data.ReadUInt32();

					byte[] compressedData = data.ReadBytes(size - 5);

					this.chunkData = DgdsUtilities.Decompress(compressionType, compressedData);
					if (this.chunkData.Length != unpackSize)
					{
						Console.WriteLine("Unpack size mismatch!");
					}
				}
				else
				{
					this.chunkData = data.ReadBytes(size);
				}
			}
		}

		// Write the chunk data to disk
		public void Write(BinaryWriter writer)
		{
			if (chunkData != null)
			{
				writer.Write(chunkData);
			}
		}

		// Print some information about this chunk to disk
		public void PrintChunk()
		{
			if (chunkData != null)
			{
				Console.WriteLine("\tChunk: {0} ({1} bytes)", identifier, chunkData.Length);
			}
			else
			{
				Console.WriteLine("\tChunk: {0}", identifier);
			}
		}
	}
}
