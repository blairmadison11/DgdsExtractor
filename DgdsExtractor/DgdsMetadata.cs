using System;
using System.Collections.Generic;
using System.Text;

namespace DgdsExtractor
{
	public enum AssetSection
	{
		ID_BIN,
		ID_DAT,
		ID_FNM,
		ID_FNT,
		ID_GAD,
		ID_INF,
		ID_MTX,
		ID_PAG,
		ID_REQ,
		ID_RES,
		ID_SCR,
		ID_SDS,
		ID_SNG,
		ID_TAG,
		ID_TT3,
		ID_TTI,
		ID_VER,
		ID_VGA,
		ID_VQT,
		ID_MA8,
		ID_DDS,
		ID_THD,
		NONE
	}

	public enum AssetType
	{
		EX_ADH,
		EX_ADL,
		EX_ADS,
		EX_AMG,
		EX_BMP,
		EX_GDS,
		EX_INS,
		EX_PAL,
		EX_FNT,
		EX_REQ,
		EX_RST,
		EX_SCR,
		EX_SDS,
		EX_SNG,
		EX_SX,
		EX_TTM,
		EX_VIN,
		EX_DAT,
		EX_DDS,
		EX_TDS,
		EX_OVL,
		NONE
	}

	public static class DgdsMetadata
	{
		private static Dictionary<string, AssetSection> stringToId = new Dictionary<string, AssetSection>()
		{
			{ "BIN", AssetSection.ID_BIN },
			{ "DAT", AssetSection.ID_DAT },
			{ "FNM", AssetSection.ID_FNM },
			{ "FNT", AssetSection.ID_FNT },
			{ "GAD", AssetSection.ID_GAD },
			{ "INF", AssetSection.ID_INF },
			{ "MTX", AssetSection.ID_MTX },
			{ "PAG", AssetSection.ID_PAG },
			{ "REQ", AssetSection.ID_REQ },
			{ "RES", AssetSection.ID_RES },
			{ "SCR", AssetSection.ID_SCR },
			{ "SDS", AssetSection.ID_SDS },
			{ "SNG", AssetSection.ID_SNG },
			{ "TAG", AssetSection.ID_TAG },
			{ "TT3", AssetSection.ID_TT3 },
			{ "TTI", AssetSection.ID_TTI },
			{ "VER", AssetSection.ID_VER },
			{ "VGA", AssetSection.ID_VGA },
			{ "VQT", AssetSection.ID_VQT },
			{ "MA8", AssetSection.ID_MA8 },
			{ "DDS", AssetSection.ID_DDS },
			{ "THD", AssetSection.ID_THD }
		};

		private static Dictionary<string, AssetType> stringToType = new Dictionary<string, AssetType>()
		{
			{ "ADH", AssetType.EX_ADH },
			{ "ADL", AssetType.EX_ADL },
			{ "ADS", AssetType.EX_ADS },
			{ "AMG", AssetType.EX_AMG },
			{ "BMP", AssetType.EX_BMP },
			{ "GDS", AssetType.EX_GDS },
			{ "INS", AssetType.EX_INS },
			{ "PAL", AssetType.EX_PAL },
			{ "FNT", AssetType.EX_FNT },
			{ "REQ", AssetType.EX_REQ },
			{ "RST", AssetType.EX_RST },
			{ "SCR", AssetType.EX_SCR },
			{ "SDS", AssetType.EX_SDS },
			{ "SNG", AssetType.EX_SNG },
			{ "SX", AssetType.EX_SX },
			{ "TTM", AssetType.EX_TTM },
			{ "VIN", AssetType.EX_VIN },
			{ "DAT", AssetType.EX_DAT },
			{ "DDS", AssetType.EX_DDS },
			{ "TDS", AssetType.EX_TDS },
			{ "OVL", AssetType.EX_OVL }
		};

		public static AssetType GetAssetType(string extension)
		{
			extension = extension.ToUpper();
			AssetType type = AssetType.NONE;
			if (stringToType.ContainsKey(extension))
			{
				type = stringToType[extension];
			}
			return type;
		}

		public static AssetSection GetAssetSection(string extension)
		{
			extension = extension.ToUpper();
			AssetSection id = AssetSection.NONE;
			if (stringToId.ContainsKey(extension))
			{
				id = stringToId[extension];
			}
			return id;
		}

		public static bool IsFlatFile(AssetType type)
		{
			return (type == AssetType.EX_RST) ||
				(type == AssetType.EX_VIN) ||
				(type == AssetType.EX_DAT);
		}

		public static bool IsPacked(AssetType type, AssetSection section)
		{
			bool packed = false;

			switch (type)
			{
				case AssetType.EX_ADS:
				case AssetType.EX_ADL:
				case AssetType.EX_ADH:
					packed = (section == AssetSection.ID_SCR);
					break;
				case AssetType.EX_BMP:
					packed = (section == AssetSection.ID_BIN) || (section == AssetSection.ID_VGA);
					break;
				case AssetType.EX_GDS:
					packed = (section == AssetSection.ID_SDS);
					break;
				case AssetType.EX_SCR:
					packed = (section == AssetSection.ID_BIN) || (section == AssetSection.ID_VGA) || (section == AssetSection.ID_MA8);
					break;
				case AssetType.EX_SDS:
					packed = (section == AssetSection.ID_SDS);
					break;
				case AssetType.EX_SNG:
					packed = (section == AssetSection.ID_SNG);
					break;
				case AssetType.EX_TTM:
					packed = (section == AssetSection.ID_TT3);
					break;
				case AssetType.EX_TDS:
					packed = (section == AssetSection.ID_THD);
					break;
				case AssetType.EX_DDS:
					packed = (section == AssetSection.ID_DDS);
					break;
				default:
					break;
			}

			return packed;
		}
	}
}
