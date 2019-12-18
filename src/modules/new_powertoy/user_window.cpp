#include "pch.h"
#include "user_window.h"

extern "C" IMAGE_DOS_HEADER __ImageBase;

UserWindow::UserWindow()
{
    state = State::DESTROYED;

    HINSTANCE hInstance = reinterpret_cast<HINSTANCE>(&__ImageBase);

    // Register the window class.
    const wchar_t CLASS_NAME[] = L"New Powertoy";

    WNDCLASS wc = {};

    wc.lpfnWndProc = WindowProc;
    wc.hInstance = hInstance;
    wc.lpszClassName = CLASS_NAME;

    RegisterClass(&wc);

    // Create the window.

    hwnd = CreateWindowEx(
        0, // Optional window styles.
        CLASS_NAME, // Window class
        L"New Powertoy", // Window text
        WS_OVERLAPPEDWINDOW, // Window style

        // Size and position
        CW_USEDEFAULT,
        CW_USEDEFAULT,
        CW_USEDEFAULT,
        CW_USEDEFAULT,

        NULL, // Parent window
        NULL, // Menu
        hInstance, // Instance handle
        NULL // Additional application data
    );
}

void UserWindow::show()
{
    ShowWindow(hwnd, SW_SHOWNORMAL);
    state = State::SHOWN;
}

void UserWindow::hide()
{
    CloseWindow(hwnd);
    state = State::HIDDEN;
}

void UserWindow::destroy()
{
    DestroyWindow(hwnd);
    state = State::DESTROYED;
}

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    switch (uMsg)
    {
    case WM_DESTROY:
        PostQuitMessage(0);
        return 0;
    case WM_PAINT: {
        PAINTSTRUCT ps;
        HDC hdc = BeginPaint(hwnd, &ps);
        FillRect(hdc, &ps.rcPaint, (HBRUSH)(COLOR_WINDOW + 1));
        EndPaint(hwnd, &ps);
    }
        return 0;
    }
    return DefWindowProc(hwnd, uMsg, wParam, lParam);
}
