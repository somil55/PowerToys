#pragma once

#include "FancyZonesDataTypes.h"

#include <common/json.h>

#include <string>
#include <vector>
#include <unordered_map>

namespace JSONHelpers
{
    namespace CanvasLayoutInfoJSON
    {
        json::JsonObject ToJson(const FancyZonesDataTypes::CanvasLayoutInfo& canvasInfo);
        std::optional<FancyZonesDataTypes::CanvasLayoutInfo> FromJson(const json::JsonObject& infoJson);
    }

    namespace GridLayoutInfoJSON
    {
        json::JsonObject ToJson(const FancyZonesDataTypes::GridLayoutInfo& gridInfo);
        std::optional<FancyZonesDataTypes::GridLayoutInfo> FromJson(const json::JsonObject& infoJson);
    }

    struct CustomZoneSetJSON
    {
        std::wstring uuid;
        FancyZonesDataTypes::CustomZoneSetData data;

        static json::JsonObject ToJson(const CustomZoneSetJSON& device);
        static std::optional<CustomZoneSetJSON> FromJson(const json::JsonObject& customZoneSet);
    };

	namespace ZoneSetDataJSON
    {
        json::JsonObject ToJson(const FancyZonesDataTypes::ZoneSetData& zoneSet);
        std::optional<FancyZonesDataTypes::ZoneSetData> FromJson(const json::JsonObject& zoneSet);
    };

    struct AppZoneHistoryJSON
    {
        std::wstring appPath;
        std::vector<FancyZonesDataTypes::AppZoneHistoryData> data;

        static json::JsonObject ToJson(const AppZoneHistoryJSON& appZoneHistory);
        static std::optional<AppZoneHistoryJSON> FromJson(const json::JsonObject& zoneSet);
    };


    struct DeviceInfoJSON
    {
        std::wstring deviceId;
        FancyZonesDataTypes::DeviceInfoData data;

        static json::JsonObject ToJson(const DeviceInfoJSON& device);
        static std::optional<DeviceInfoJSON> FromJson(const json::JsonObject& device);
    };

    using TAppZoneHistoryMap = std::unordered_map<std::wstring, std::vector<FancyZonesDataTypes::AppZoneHistoryData>>;
    using TDeviceInfoMap = std::unordered_map<std::wstring, FancyZonesDataTypes::DeviceInfoData>;
    using TCustomZoneSetsMap = std::unordered_map<std::wstring, FancyZonesDataTypes::CustomZoneSetData>;

    json::JsonObject GetPersistFancyZonesJSON(const std::wstring& zonesSettingsFileName, const std::wstring& appZoneHistoryFileName);
    void SaveFancyZonesData(const std::wstring& zonesSettingsFileName,
							const std::wstring& appZoneHistoryFileName,
							const TDeviceInfoMap& deviceInfoMap,
							const TCustomZoneSetsMap& customZoneSetsMap,
							const TAppZoneHistoryMap& appZoneHistoryMap);

    TAppZoneHistoryMap ParseAppZoneHistory(const json::JsonObject& fancyZonesDataJSON);
    json::JsonArray SerializeAppZoneHistory(const TAppZoneHistoryMap& appZoneHistoryMap);

    TDeviceInfoMap ParseDeviceInfos(const json::JsonObject& fancyZonesDataJSON);
    json::JsonArray SerializeDeviceInfos(const TDeviceInfoMap& deviceInfoMap);

    TCustomZoneSetsMap ParseCustomZoneSets(const json::JsonObject& fancyZonesDataJSON);
    json::JsonArray SerializeCustomZoneSets(const TCustomZoneSetsMap& customZoneSetsMap);

    void SerializeDeviceInfoToTmpFile(const JSONHelpers::DeviceInfoJSON& deviceInfo, std::wstring_view tmpFilePath);
    std::optional<DeviceInfoJSON> ParseDeviceInfoFromTmpFile(std::wstring_view tmpFilePath);
    std::optional<CustomZoneSetJSON> ParseCustomZoneSetFromTmpFile(std::wstring_view tmpFilePath);
    std::vector<std::wstring> ParseDeletedCustomZoneSetsFromTmpFile(std::wstring_view tmpFilePath);
}
