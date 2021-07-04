using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DgdsExtractor
{
	class DgdsVolume
	{
		private string directory, filename;
		private DgdsAsset[] assets;

		public DgdsVolume(string directory, string fileName, int numAssets)
		{
			this.directory = directory;
			this.filename = fileName;
			assets = new DgdsAsset[numAssets];
		}

		public string Filename { get => filename; set => filename = value; }
		public int NumAssets { get => assets.Length; }

		public void InitializeAsset(int index, int fileHash, uint size)
		{
			assets[index] = new DgdsAsset(fileHash, size);
		}

		public void ReadAssets()
		{
			using (BinaryReader file = new BinaryReader(File.OpenRead(directory + filename)))
			{
				for (int i = 0; i < assets.Length; ++i)
				{
					assets[i].ReadAsset(file);
				}
			}
		}

		public void WriteAssets(string path)
		{
			for (int i = 0; i < assets.Length; ++i)
			{
				assets[i].Write(path);
			}
		}

		public void PrintAssets()
		{
			for (int i = 0; i < assets.Length; ++i)
			{
				assets[i].PrintAsset();
			}
		}
	}
}
