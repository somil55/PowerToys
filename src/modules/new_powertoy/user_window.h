#pragma once
#include <interface/powertoy_module_interface.h>
#include <interface/lowlevel_keyboard_event_data.h>
#include <interface/win_hook_event_data.h>

// Callback function for window process
LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

// Manage window
class UserWindow
{
public:
    UserWindow();
    void show();
    void hide();
    void destroy();
    enum class State
    {
        HIDDEN,
        SHOWN,
        DESTROYED
    };
    State state;

private:
    // Store Handle for window
    HWND hwnd = NULL;
};