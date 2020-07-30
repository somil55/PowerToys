// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <common/common.h>
#include <interface/powertoy_module_interface.h>
#include "trace.h"
#include "resource.h"
#include <common\settings_objects.h>
#include <common\os-detect.h>

extern "C" IMAGE_DOS_HEADER __ImageBase;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        Trace::RegisterProvider();
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        Trace::UnregisterProvider();
        break;
    }
    return TRUE;
}

struct ModuleSettings{
} g_settings;

class ColorPicker : public PowertoyModuleIface
{
private:
    bool m_enabled = false;

    std::wstring app_name;

    HANDLE m_hProcess;

     // Time to wait for process to close after sending WM_CLOSE signal
    static const int MAX_WAIT_MILLISEC = 10000;
public:
    ColorPicker()
    {
        app_name = GET_RESOURCE_STRING(IDS_LAUNCHER_NAME);
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

    // Return the display name of the powertoy, this will be cached by the runner
    virtual const wchar_t* get_name() override
    {
        return app_name.c_str();
    }

    virtual const wchar_t** get_events() override
    {
        static const wchar_t* events[] = { nullptr };

        return events;
    }

    virtual bool get_config(wchar_t* buffer, int* buffer_size) override
    {
        HINSTANCE hinstance = reinterpret_cast<HINSTANCE>(&__ImageBase);

        // Create a Settings object.
        PowerToysSettings::Settings settings(hinstance, get_name());
        settings.set_description(GET_RESOURCE_STRING(IDS_LAUNCHER_SETTINGS_DESC));

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
                PowerToysSettings::PowerToyValues::from_json_string(config);

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

    virtual void enable(){

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

    virtual void disable() {
        if (m_enabled)
        {
            terminateProcess();
        }
        
        m_enabled = false;
    }

    virtual bool is_enabled() override
    {
        return m_enabled;
    }

    virtual intptr_t signal_event(const wchar_t* name, intptr_t data) override
    {
        return 0;
    }

    static BOOL CALLBACK requestMainWindowClose(HWND nextWindow, LPARAM closePid)
    {
        DWORD windowPid;
        GetWindowThreadProcessId(nextWindow, &windowPid);

        if (windowPid == (DWORD)closePid)
            ::PostMessage(nextWindow, WM_CLOSE, 0, 0);

        return true;
    }

    void terminateProcess()
    {
        DWORD processID = GetProcessId(m_hProcess);
        EnumWindows(&requestMainWindowClose, processID);
        const DWORD result = WaitForSingleObject(m_hProcess, MAX_WAIT_MILLISEC);
        if (result == WAIT_TIMEOUT)
        {
            TerminateProcess(m_hProcess, 1);
        }
    }

     /* Register helper class to handle system menu items related actions. */
    virtual void register_system_menu_helper(PowertoySystemMenuIface* helper) {}
    /* Handle action on system menu item. */
    virtual void signal_system_menu_action(const wchar_t* name) {}
};

extern "C" __declspec(dllexport) PowertoyModuleIface* __cdecl powertoy_create()
{
    return new ColorPicker();
}

