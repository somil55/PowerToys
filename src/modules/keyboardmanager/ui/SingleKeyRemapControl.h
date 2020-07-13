#pragma once
#include "KeyDropDownControl.h"

class KeyboardManagerState;
namespace winrt::Windows::UI::Xaml
{
    struct XamlRoot;
    namespace Controls
    {
        struct StackPanel;
        struct Grid;
    }
}

class SingleKeyRemapControl
{
private:
    // Drop down to display the selected remap key
    KeyDropDownControl singleKeyRemapDropDown;

    // Button to type the remap key
    winrt::Windows::Foundation::IInspectable typeKey;

    // StackPanel to parent the above controls
    winrt::Windows::Foundation::IInspectable singleKeyRemapControlLayout;

public:
    // Handle to the current Edit Keyboard Window
    static HWND EditKeyboardWindowHandle;
    // Pointer to the keyboard manager state
    static KeyboardManagerState* keyboardManagerState;
    // Stores the current list of remappings
    static std::vector<std::vector<DWORD>> singleKeyRemapBuffer;

    // constructor
    SingleKeyRemapControl(Grid table, const int colIndex);

    // Function to add a new row to the remap keys table. If the originalKey and newKey args are provided, then the displayed remap keys are set to those values.
    static void AddNewControlKeyRemapRow(winrt::Windows::UI::Xaml::Controls::Grid& parent, std::vector<std::vector<std::unique_ptr<SingleKeyRemapControl>>>& keyboardRemapControlObjects, const DWORD originalKey = NULL, const DWORD newKey = NULL);

    // Function to return the stack panel element of the SingleKeyRemapControl. This is the externally visible UI element which can be used to add it to other layouts
    winrt::Windows::UI::Xaml::Controls::StackPanel getSingleKeyRemapControl();

    // Function to create the detect remap keys UI window
    void createDetectKeyWindow(winrt::Windows::Foundation::IInspectable const& sender, XamlRoot xamlRoot, std::vector<std::vector<DWORD>>& singleKeyRemapBuffer, KeyboardManagerState& keyboardManagerState);
};
