using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DgdsExtractor
{
	class DgdsChunk
	{
		const int ID_LENGTH = 4;

		private AssetType chunkType = AssetType.NONE;
		private AssetSection section = AssetSection.NONE;
		private byte[] chunkData;
		private bool isContainer;
		private List<string> textLines;

		public bool IsContainer { get => isContainer; }
		public AssetType ChunkType { get => chunkType; }

		public DgdsChunk(AssetType type)
		{
			this.chunkType = type;
		}

		// Parses the chunk data from the specified asset data
		public void ReadChunk(BinaryReader data)
		{
			byte[] id = data.ReadBytes(ID_LENGTH);
			if (id[3] != ':')
			{
				throw new Exception("Invalid header!");
			}

			string idStr = "";
			if (id[2] == (byte)0)
			{
				idStr = string.Concat(Convert.ToChar(id[0]), Convert.ToChar(id[1]));
			}
			else
			{
				idStr = string.Concat(Convert.ToChar(id[0]), Convert.ToChar(id[1]), Convert.ToChar(id[2]));
			}

			if (chunkType == AssetType.NONE)
			{
				chunkType = DgdsMetadata.GetAssetType(idStr);
			}
			else
			{
				section = DgdsMetadata.GetAssetSection(idStr);
			}

			uint sizeData = data.ReadUInt32();
			this.isContainer = Convert.ToBoolean(sizeData >> 31);
			int size = Convert.ToInt32(sizeData & 0x7FFFFFFF);
			
			if (!isContainer)
			{
				if (DgdsMetadata.IsCompressed(ChunkType, section))
				{
					byte compressionType = data.ReadByte();
					uint unpackSize = data.ReadUInt32();

					byte[] compressedData = data.ReadBytes(size - 5);

					this.chunkData = DgdsUtilities.Decompress(compressionType, compressedData);
				}
				else
				{
					this.chunkData = data.ReadBytes(size);
				}
			}

			if (chunkType == AssetType.SDS && section == AssetSection.SDS)
			{
				ExtractText();
			}
		}

		private void ExtractText()
		{
			textLines = new List<string>();
			int index = 13;
			while (index + 6 < chunkData.Length)
			{
				if (chunkData[index++] == 0x04)
				{
					ushort op0 = BitConverter.ToUInt16(chunkData, index - 1);
					ushort op1 = BitConverter.ToUInt16(chunkData, index + 1);
					ushort op2 = BitConverter.ToUInt16(chunkData, index + 3);

					if (op0 == 0x04 && (op1 == 0x02 || op1 == 0x03) && (op2 == 0x00 || op2 == 0x80))
					{
						index += 11;
						ushort count = BitConverter.ToUInt16(chunkData, index);
						index += 2;
						string line = Encoding.ASCII.GetString(chunkData, index, Array.IndexOf(chunkData, (byte)0, index, count) - index).Trim().Replace('\r', '\n');
						if (line != "")
						{
							textLines.Add(line);
						}
						index += count;
					}
				}
			}
		}

		// Write all text lines (dialogue, descriptions, etc.) contained in this chunk to disk
		public void WriteText(StreamWriter writer)
		{
			if (textLines != null)
			{
				foreach (string line in textLines)
				{
					writer.Write("{0}\n\n", line);
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

		// Print some information about this chunk to console
		public void PrintChunk()
		{
			if (chunkData != null)
			{
				Console.WriteLine("\tChunk: {0} ({1} bytes)", this, chunkData.Length);
			}
			else
			{
				Console.WriteLine("\tChunk: {0}", this);
			}
		}

		public override string ToString()
		{
			return section == AssetSection.NONE ? chunkType.ToString() : section.ToString();
		}
	}
}
