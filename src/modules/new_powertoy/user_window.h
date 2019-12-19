#pragma once

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
    void displayMessageBox(wchar_t* message);

private:
    // Store Handle for window
    HWND hwnd = NULL;
};