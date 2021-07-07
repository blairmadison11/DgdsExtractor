using System.Collections.Generic;
using System.Linq;

namespace DgdsExtractor
{
	public enum AssetSection {
		NONE, BIN, DAT, FNM, FNT, GAD, INF, MTX,
		PAG, REQ, RES, SCR, SDS, SNG, TAG, TT3,
		TTI, VER, VGA, VQT, MA8, DDS, THD}

	public enum AssetType {
		NONE, ADH, ADL, ADS, AMG, BMP, GDS, INS,
		PAL, FNT, REQ, RST, SCR, SDS, SNG,
		SX, TTM, VIN, DAT, DDS, TDS, OVL }

	public static class DgdsMetadata
	{
		private static readonly Dictionary<string, AssetSection> SectionStringMap = new Dictionary<string, AssetSection>()
		{
			{ "BIN", AssetSection.BIN }, { "DAT", AssetSection.DAT }, { "FNM", AssetSection.FNM },
			{ "FNT", AssetSection.FNT }, { "GAD", AssetSection.GAD }, { "INF", AssetSection.INF },
			{ "MTX", AssetSection.MTX }, { "PAG", AssetSection.PAG }, { "REQ", AssetSection.REQ },
			{ "RES", AssetSection.RES }, { "SCR", AssetSection.SCR }, { "SDS", AssetSection.SDS },
			{ "SNG", AssetSection.SNG }, { "TAG", AssetSection.TAG }, { "TT3", AssetSection.TT3 },
			{ "TTI", AssetSection.TTI }, { "VER", AssetSection.VER }, { "VGA", AssetSection.VGA },
			{ "VQT", AssetSection.VQT }, { "MA8", AssetSection.MA8 }, { "DDS", AssetSection.DDS },
			{ "THD", AssetSection.THD }
		};

		private static readonly Dictionary<string, AssetType> TypeStringMap = new Dictionary<string, AssetType>()
		{
			{ "ADH", AssetType.ADH }, { "ADL", AssetType.ADL }, { "ADS", AssetType.ADS },
			{ "AMG", AssetType.AMG }, { "BMP", AssetType.BMP }, { "GDS", AssetType.GDS },
			{ "INS", AssetType.INS }, { "PAL", AssetType.PAL }, { "FNT", AssetType.FNT },
			{ "REQ", AssetType.REQ }, { "RST", AssetType.RST }, { "SCR", AssetType.SCR },
			{ "SDS", AssetType.SDS }, { "SNG", AssetType.SNG }, { "SX", AssetType.SX },
			{ "TTM", AssetType.TTM }, { "VIN", AssetType.VIN }, { "DAT", AssetType.DAT },
			{ "DDS", AssetType.DDS }, { "TDS", AssetType.TDS }, { "OVL", AssetType.OVL }
		};

		// converts identifier string to corresponding AssetType enum
		public static AssetType GetAssetType(string identifier)
		{
			return TypeStringMap.GetValueOrDefault(identifier.ToUpper());
		}

		// converts identifier string to corresponding AssetSection enum
		public static AssetSection GetAssetSection(string identifier)
		{
			return SectionStringMap.GetValueOrDefault(identifier.ToUpper());
		}

		// determines whether specified AssetType is a flat file or not
		public static bool IsFlatFile(AssetType type)
		{
			return (type == AssetType.RST) ||
				(type == AssetType.VIN) ||
				(type == AssetType.DAT);
		}

		// determines whether the specified asset section within the specified asset type is compressed or not
		public static bool IsCompressed(AssetType type, AssetSection section)
		{
			bool compressed = false;

			switch (type)
			{
				case AssetType.ADS:
				case AssetType.ADL:
				case AssetType.ADH:
					compressed = (section == AssetSection.SCR);
					break;
				case AssetType.BMP:
					compressed = (section == AssetSection.BIN) || (section == AssetSection.VGA);
					break;
				case AssetType.GDS:
					compressed = (section == AssetSection.SDS);
					break;
				case AssetType.SCR:
					compressed = (section == AssetSection.BIN) || (section == AssetSection.VGA) || (section == AssetSection.MA8);
					break;
				case AssetType.SDS:
					compressed = (section == AssetSection.SDS);
					break;
				case AssetType.SNG:
					compressed = (section == AssetSection.SNG);
					break;
				case AssetType.TTM:
					compressed = (section == AssetSection.TT3);
					break;
				case AssetType.TDS:
					compressed = (section == AssetSection.THD);
					break;
				case AssetType.DDS:
					compressed = (section == AssetSection.DDS);
					break;
				default:
					break;
			}

			return compressed;
		}
	}
}
