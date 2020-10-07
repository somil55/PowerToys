// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <common/common.h>
#include <interface/powertoy_module_interface.h>
#include "trace.h"
#include "Generated Files/resource.h"
#include <common\settings_objects.h>
#include <common\os-detect.h>

extern "C" IMAGE_DOS_HEADER __ImageBase;

BOOL APIENTRY DllMain(HMODULE hModule,
                      DWORD ul_reason_for_call,
                      LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        Trace::RegisterProvider();
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        Trace::UnregisterProvider();
        break;
    }
    return TRUE;
}

struct ModuleSettings
{
} g_settings;

class ColorPicker : public PowertoyModuleIface
{
private:
    bool m_enabled = false;

    std::wstring app_name;

    //contains the non localized key of the powertoy
    std::wstring app_key = L"ColorPicker";

    HANDLE m_hProcess;

    // Time to wait for process to close after sending WM_CLOSE signal
    static const int MAX_WAIT_MILLISEC = 10000;

public:
    ColorPicker()
    {
        app_name = GET_RESOURCE_STRING(IDS_COLORPICKER_NAME);
    }

    ~ColorPicker()
    {
        if (m_enabled)
        {
        }
        m_enabled = false;
    }

    // Destroy the powertoy and free memory
    virtual void destroy() override
    {
        delete this;
    }

    // Return the localized display name of the powertoy
    virtual const wchar_t* get_name() override
    {
        return app_name.c_str();
    }

    // Return the non localized key of the powertoy, this will be cached by the runner
    virtual const wchar_t* get_key() override
    {
        return app_key.c_str();
    }

    virtual bool get_config(wchar_t* buffer, int* buffer_size) override
    {
        HINSTANCE hinstance = reinterpret_cast<HINSTANCE>(&__ImageBase);

        // Create a Settings object.
        PowerToysSettings::Settings settings(hinstance, get_name());
        settings.set_description(GET_RESOURCE_STRING(IDS_COLORPICKER_SETTINGS_DESC));

        settings.set_overview_link(L"https://aka.ms/PowerToysOverview_ColorPicker");

        return settings.serialize_to_buffer(buffer, buffer_size);
    }

    virtual void call_custom_action(const wchar_t* action) override
    {
    }

    virtual void set_config(const wchar_t* config) override
    {
        try
        {
            // Parse the input JSON string.
            PowerToysSettings::PowerToyValues values =
                PowerToysSettings::PowerToyValues::from_json_string(config, get_key());

            // If you don't need to do any custom processing of the settings, proceed
            // to persists the values calling:
            values.save_to_settings_file();
            // Otherwise call a custom function to process the settings before saving them to disk:
            // save_settings();
        }
        catch (std::exception ex)
        {
            // Improper JSON.
        }
    }

    virtual void enable()
    {
        // use only with new settings?
        if (UseNewSettings())
        {
            unsigned long powertoys_pid = GetCurrentProcessId();

            std::wstring executable_args = L"";
            executable_args.append(std::to_wstring(powertoys_pid));

            SHELLEXECUTEINFOW sei{ sizeof(sei) };
            sei.fMask = { SEE_MASK_NOCLOSEPROCESS | SEE_MASK_FLAG_NO_UI };
            sei.lpFile = L"modules\\ColorPicker\\ColorPicker.exe";
            sei.nShow = SW_SHOWNORMAL;
            sei.lpParameters = executable_args.data();
            ShellExecuteExW(&sei);

            m_hProcess = sei.hProcess;

            m_enabled = true;
        }
    };

    virtual void disable()
    {
        if (m_enabled)
        {
            TerminateProcess(m_hProcess, 1);
        }

        m_enabled = false;
    }

    virtual bool is_enabled() override
    {
        return m_enabled;
    }
};

extern "C" __declspec(dllexport) PowertoyModuleIface* __cdecl powertoy_create()
{
    return new ColorPicker();
}
