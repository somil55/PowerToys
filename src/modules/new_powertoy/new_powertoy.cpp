#include "pch.h"
#include <interface/lowlevel_keyboard_event_data.h>
#include <interface/win_hook_event_data.h>
#include <common\settings_objects.h>
#include "new_powertoy.h"
#include "user_window.h"
#include "target_state.h"

extern "C" IMAGE_DOS_HEADER __ImageBase;

NewPowertoy* instance = nullptr;

// The PowerToy name that will be shown in the settings.
const static wchar_t* MODULE_NAME = L"New Powertoy";

// Add a description that will we shown in the module settings page.
const static wchar_t* MODULE_DESC = L"New Powertoy";

// Implement the PowerToy Module Interface and all the required methods.

NewPowertoy::NewPowertoy()
{
    m_enabled = false;
    state = State::DESTROYED;
    init_settings();
};

// Return the display name of the powertoy, this will be cached by the runner
const wchar_t* NewPowertoy::get_name()
{
    return MODULE_NAME;
}

// Return array of the names of all events that this powertoy listens for, with
// nullptr as the last element of the array. Nullptr can also be retured for empty
// list.
const wchar_t** NewPowertoy::get_events()
{
    static const wchar_t* events[] = { ll_keyboard, 0 };
    return events;
}

// Return JSON with the configuration options.
bool NewPowertoy::get_config(wchar_t* buffer, int* buffer_size)
{
    HINSTANCE hinstance = reinterpret_cast<HINSTANCE>(&__ImageBase);

    // Create a Settings object.
    PowerToysSettings::Settings settings(hinstance, get_name());
    settings.set_description(MODULE_DESC);

    return settings.serialize_to_buffer(buffer, buffer_size);
}

// Called by the runner to pass the updated settings values as a serialized JSON.
void NewPowertoy::set_config(const wchar_t* config)
{
    try
    {
        // Parse the input JSON string.
        PowerToysSettings::PowerToyValues values =
            PowerToysSettings::PowerToyValues::from_json_string(config);
        values.save_to_settings_file();
    }
    catch (std::exception ex)
    {
        // Improper JSON.
    }
}

// Enable the powertoy
void NewPowertoy::enable()
{
    m_enabled = true;
}

// Disable the powertoy
void NewPowertoy::disable()
{
    m_enabled = false;
}

// Returns if the powertoys is enabled
bool NewPowertoy::is_enabled()
{
    return m_enabled;
}

// Handle incoming event, data is event-specific
intptr_t NewPowertoy::signal_event(const wchar_t* name, intptr_t data)
{
    if (m_enabled && wcscmp(name, ll_keyboard) == 0)
    {
        auto& event = *(reinterpret_cast<LowlevelKeyboardEvent*>(data));
        if (event.wParam == WM_KEYDOWN ||
            event.wParam == WM_SYSKEYDOWN ||
            event.wParam == WM_KEYUP ||
            event.wParam == WM_SYSKEYUP)
        {
            if (event.lParam->vkCode == 0x42 && state != State::SHOWN)
            {
                if (state == State::DESTROYED)
                {
                    create();
                }
                show();
            }
            else if (event.lParam->vkCode == 0x43 && state == State::SHOWN)
            {
                hide();
            }
            else if (event.lParam->vkCode == 0x44 || event.lParam->vkCode == 0x45)
            {
                target_state->signal_event(event.lParam->vkCode);
            }
        }
    }
    return 0;
}

void NewPowertoy::create()
{
    window = std::make_unique<UserWindow>();
    target_state = std::make_unique<TargetState>();
    state = state = State::CREATED;
}

void NewPowertoy::show()
{
    window->show();
    state = State::SHOWN;
}

void NewPowertoy::hide()
{
    window->hide();
    state = state = State::HIDDEN;
}

void NewPowertoy::destroy()
{
    state = State::DESTROYED;
    target_state->exit();
    window->destroy();
    target_state.reset(nullptr);
    window.reset(nullptr);
}

void NewPowertoy::displayMessageBox(wchar_t* message)
{
    window->displayMessageBox(message);
}

// Load the settings file.
void NewPowertoy::init_settings()
{
    try
    {
        // Load and parse the settings file for this PowerToy.
        PowerToysSettings::PowerToyValues settings =
            PowerToysSettings::PowerToyValues::load_from_settings_file(get_name());
    }
    catch (std::exception ex)
    {
        // Error while loading from the settings file. Let default values stay as they are.
    }
}

/* Register helper class to handle system menu items related actions. */
void register_system_menu_helper(PowertoySystemMenuIface* helper){};

/* Handle action on system menu item. */
void signal_system_menu_action(const wchar_t* name){};
