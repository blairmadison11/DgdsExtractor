using System;
using System.IO;

namespace DgdsExtractor
{
	class DgdsVolume
	{
		private readonly string directory, filename;
		private readonly DgdsAsset[] assets;

		public DgdsVolume(string directory, string filename, int numAssets)
		{
			this.directory = directory;
			this.filename = filename;
			assets = new DgdsAsset[numAssets];
		}

		// Allocate memory for the asset at specified index
		// Intialize data with specified file size
		public void InitializeAsset(int index, uint size)
		{
			assets[index] = new DgdsAsset(size);
		}

		// Read all asset data from the volume file
		public void ReadAssets()
		{
			using BinaryReader inFile = new BinaryReader(File.OpenRead(directory + filename));
			foreach (DgdsAsset asset in assets)
			{
				asset.ReadAsset(inFile);
			}
		}

		// Write all extracted asset data to disk
		public void WriteData(string path)
		{
			foreach (DgdsAsset asset in assets)
			{
				asset.WriteData(path);
			}
		}

		// Write all text lines (dialogue, descriptions, etc.) contained in this volume to disk
		public void WriteText(StreamWriter outFile)
		{
			foreach (DgdsAsset asset in assets)
			{
				asset.WriteText(outFile);
			}
		}

		// Print a summary of asset information to the console
		public void Print()
		{
			Console.WriteLine("\n{0} contains {1} assets\n", filename, assets.Length);
			foreach (DgdsAsset asset in assets)
			{
				asset.Print();
			}
		}
	}
}
