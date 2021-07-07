using System;
using System.IO;

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

		// Allocate memory for the asset at specified index
		// Intialize data with specified filename hash and file size
		public void InitializeAsset(int index, int fileHash, uint size)
		{
			assets[index] = new DgdsAsset(fileHash, size);
		}

		// Read all asset data from the volume file
		public void ReadAssets()
		{
			using BinaryReader file = new BinaryReader(File.OpenRead(directory + filename));
			foreach (DgdsAsset asset in assets)
			{
				asset.ReadAsset(file);
			}
		}

		// Write all extracted asset data to disk
		public void WriteAssets(string path)
		{
			foreach (DgdsAsset asset in assets)
			{
				asset.Write(path);
			}
		}

		// Write all text lines (dialogue, descriptions, etc.) contained in this volume to disk
		public void WriteText(StreamWriter writer)
		{
			foreach (DgdsAsset asset in assets)
			{
				asset.WriteText(writer);
			}
		}

		// Print a summary of asset information to the console
		public void PrintAssets()
		{
			Console.WriteLine("\n{0} contains {1} assets\n", filename, assets.Length);
			foreach (DgdsAsset asset in assets)
			{
				asset.PrintAsset();
			}
		}
	}
}
