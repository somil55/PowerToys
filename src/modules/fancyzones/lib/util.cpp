#include "pch.h"
#include "util.h"
#include "Settings.h"

#include <common/common.h>
#include <common/dpi_aware.h>

#include <sstream>

namespace
{
    const wchar_t POWER_TOYS_APP_POWER_LAUCHER[] = L"POWERLAUNCHER.EXE";
    const wchar_t POWER_TOYS_APP_FANCY_ZONES_EDITOR[] = L"FANCYZONESEDITOR.EXE";
}

typedef BOOL(WINAPI* GetDpiForMonitorInternalFunc)(HMONITOR, UINT, UINT*, UINT*);
UINT GetDpiForMonitor(HMONITOR monitor) noexcept
{
    UINT dpi{};
    if (wil::unique_hmodule user32{ LoadLibrary(L"user32.dll") })
    {
        if (auto func = reinterpret_cast<GetDpiForMonitorInternalFunc>(GetProcAddress(user32.get(), "GetDpiForMonitorInternal")))
        {
            func(monitor, 0, &dpi, &dpi);
        }
    }

    if (dpi == 0)
    {
        if (wil::unique_hdc hdc{ GetDC(nullptr) })
        {
            dpi = GetDeviceCaps(hdc.get(), LOGPIXELSX);
        }
    }

    return (dpi == 0) ? DPIAware::DEFAULT_DPI : dpi;
}

void OrderMonitors(std::vector<std::pair<HMONITOR, RECT>>& monitorInfo)
{
    const size_t nMonitors = monitorInfo.size();
    // blocking[i][j] - whether monitor i blocks monitor j in the ordering, i.e. monitor i should go before monitor j
    std::vector<std::vector<bool>> blocking(nMonitors, std::vector<bool>(nMonitors, false));

    // blockingCount[j] - the number of monitors which block monitor j
    std::vector<size_t> blockingCount(nMonitors, 0);

    for (size_t i = 0; i < nMonitors; i++)
    {
        RECT rectI = monitorInfo[i].second;
        for (size_t j = 0; j < nMonitors; j++)
        {
            RECT rectJ = monitorInfo[j].second;
            blocking[i][j] = rectI.top < rectJ.bottom && rectI.left < rectJ.right && i != j;
            if (blocking[i][j])
            {
                blockingCount[j]++;
            }
        }
    }

    // used[i] - whether the sorting algorithm has used monitor i so far
    std::vector<bool> used(nMonitors, false);

    // the sorted sequence of monitors
    std::vector<std::pair<HMONITOR, RECT>> sortedMonitorInfo;

    for (size_t iteration = 0; iteration < nMonitors; iteration++)
    {
        // Indices of candidates to become the next monitor in the sequence
        std::vector<size_t> candidates;

        // First, find indices of all unblocked monitors
        for (size_t i = 0; i < nMonitors; i++)
        {
            if (blockingCount[i] == 0 && !used[i])
            {
                candidates.push_back(i);
            }
        }

        // In the unlikely event that there are no unblocked monitors, declare all unused monitors as candidates.
        if (candidates.empty())
        {
            for (size_t i = 0; i < nMonitors; i++)
            {
                if (!used[i])
                {
                    candidates.push_back(i);
                }
            }
        }

        // Pick the lexicographically smallest monitor as the next one
        size_t smallest = candidates[0];
        for (size_t j = 1; j < candidates.size(); j++)
        {
            size_t current = candidates[j];

            // Compare (top, left) lexicographically
            if (std::tie(monitorInfo[current].second.top, monitorInfo[current].second.left) <
                std::tie(monitorInfo[smallest].second.top, monitorInfo[smallest].second.left))
            {
                smallest = current;
            }
        }

        used[smallest] = true;
        sortedMonitorInfo.push_back(monitorInfo[smallest]);
        for (size_t i = 0; i < nMonitors; i++)
        {
            if (blocking[smallest][i])
            {
                blockingCount[i]--;
            }
        }
    }

    monitorInfo = std::move(sortedMonitorInfo);
}

void SizeWindowToRect(HWND window, RECT rect) noexcept
{
    WINDOWPLACEMENT placement{};
    ::GetWindowPlacement(window, &placement);

    //wait if SW_SHOWMINIMIZED would be removed from window (Issue #1685)
    for (int i = 0; i < 5 && (placement.showCmd & SW_SHOWMINIMIZED) != 0; i++)
    {
        std::this_thread::sleep_for(std::chrono::milliseconds(100));
        ::GetWindowPlacement(window, &placement);
    }

    // Do not restore minimized windows. We change their placement though so they restore to the correct zone.
    if ((placement.showCmd & SW_SHOWMINIMIZED) == 0)
    {
        placement.showCmd = SW_RESTORE | SW_SHOWNA;
    }

    // Remove maximized show command to make sure window is moved to the correct zone.
    if (placement.showCmd & SW_SHOWMAXIMIZED)
    {
        placement.showCmd = SW_RESTORE;
        placement.flags &= ~WPF_RESTORETOMAXIMIZED;
    }

    placement.rcNormalPosition = rect;
    placement.flags |= WPF_ASYNCWINDOWPLACEMENT;

    ::SetWindowPlacement(window, &placement);
    // Do it again, allowing Windows to resize the window and set correct scaling
    // This fixes Issue #365
    ::SetWindowPlacement(window, &placement);
}

bool IsInterestingWindow(HWND window, const std::vector<std::wstring>& excludedApps) noexcept
{
    auto filtered = get_fancyzones_filtered_window(window);
    if (!filtered.zonable)
    {
        return false;
    }
    // Filter out user specified apps
    CharUpperBuffW(filtered.process_path.data(), (DWORD)filtered.process_path.length());
    if (find_app_name_in_path(filtered.process_path, excludedApps))
    {
        return false;
    }
    if (find_app_name_in_path(filtered.process_path, { POWER_TOYS_APP_POWER_LAUCHER }))
    {
        return false;
    }
    if (find_app_name_in_path(filtered.process_path, { POWER_TOYS_APP_FANCY_ZONES_EDITOR }))
    {
        return false;
    }
    return true;
}

void SaveWindowSizeAndOrigin(HWND window) noexcept
{
    HANDLE handle = GetPropW(window, RESTORE_SIZE_STAMP);
    if (handle)
    {
        // Size already set, skip
        return;
    }

    RECT rect;
    if (GetWindowRect(window, &rect))
    {
        int width = rect.right - rect.left;
        int height = rect.bottom - rect.top;
        int originX = rect.left;
        int originY = rect.top;

        DPIAware::InverseConvert(MonitorFromWindow(window, MONITOR_DEFAULTTONULL), width, height);
        DPIAware::InverseConvert(MonitorFromWindow(window, MONITOR_DEFAULTTONULL), originX, originY);

        std::array<int, 2> windowSizeData = { width, height };
        std::array<int, 2> windowOriginData = { originX, originY };
        HANDLE rawData;
        memcpy(&rawData, windowSizeData.data(), sizeof rawData);
        SetPropW(window, RESTORE_SIZE_STAMP, rawData);
        memcpy(&rawData, windowOriginData.data(), sizeof rawData);
        SetPropW(window, RESTORE_ORIGIN_STAMP, rawData);
    }
}

void RestoreWindowSize(HWND window) noexcept
{
    auto windowSizeData = GetPropW(window, RESTORE_SIZE_STAMP);
    if (windowSizeData)
    {
        std::array<int, 2> windowSize;
        memcpy(windowSize.data(), &windowSizeData, sizeof windowSize);

        // {width, height}
        DPIAware::Convert(MonitorFromWindow(window, MONITOR_DEFAULTTONULL), windowSize[0], windowSize[1]);

        RECT rect;
        if (GetWindowRect(window, &rect))
        {
            rect.right = rect.left + windowSize[0];
            rect.bottom = rect.top + windowSize[1];
            SizeWindowToRect(window, rect);
        }

        ::RemoveProp(window, RESTORE_SIZE_STAMP);
    }
}

void RestoreWindowOrigin(HWND window) noexcept
{
    auto windowOriginData = GetPropW(window, RESTORE_ORIGIN_STAMP);
    if (windowOriginData)
    {
        std::array<int, 2> windowOrigin;
        memcpy(windowOrigin.data(), &windowOriginData, sizeof windowOrigin);

        // {width, height}
        DPIAware::Convert(MonitorFromWindow(window, MONITOR_DEFAULTTONULL), windowOrigin[0], windowOrigin[1]);

        RECT rect;
        if (GetWindowRect(window, &rect))
        {
            int xOffset = windowOrigin[0] - rect.left;
            int yOffset = windowOrigin[1] - rect.top;

            rect.left += xOffset;
            rect.right += xOffset;
            rect.top += yOffset;
            rect.bottom += yOffset;
            SizeWindowToRect(window, rect);
        }

        ::RemoveProp(window, RESTORE_ORIGIN_STAMP);
    }
}

bool IsValidGuid(const std::wstring& str)
{
    GUID id;
    return SUCCEEDED(CLSIDFromString(str.c_str(), &id));
}

bool IsValidDeviceId(const std::wstring& str)
{
    std::wstring monitorName;
    std::wstring temp;
    std::vector<std::wstring> parts;
    std::wstringstream wss(str);

    /* 
        Important fix for device info that contains a '_' in the name:
        1. first search for '#'
        2. Then split the remaining string by '_'
    */

    // Step 1: parse the name until the #, then to the '_'
    if (str.find(L'#') != std::string::npos)
    {
        std::getline(wss, temp, L'#');

        monitorName = temp;

        if (!std::getline(wss, temp, L'_'))
        {
            return false;
        }

        monitorName += L"#" + temp;
        parts.push_back(monitorName);
    }

    // Step 2: parse the rest of the id
    while (std::getline(wss, temp, L'_'))
    {
        parts.push_back(temp);
    }

    if (parts.size() != 4)
    {
        return false;
    }

    /*
        Refer to ZoneWindowUtils::GenerateUniqueId parts contain:
        1. monitor id [string]
        2. width of device [int]
        3. height of device [int]
        4. virtual desktop id (GUID) [string]
    */
    try
    {
        //check if resolution contain only digits
        for (const auto& c : parts[1])
        {
            std::stoi(std::wstring(&c));
        }
        for (const auto& c : parts[2])
        {
            std::stoi(std::wstring(&c));
        }
    }
    catch (const std::exception&)
    {
        return false;
    }

    if (!IsValidGuid(parts[3]) || parts[0].empty())
    {
        return false;
    }

    return true;
}
