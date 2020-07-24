#include "pch.h"
#include "SingleKeyRemapControl.h"
#include "keyboardmanager/common/Helpers.h"
#include "keyboardmanager/common/KeyboardManagerConstants.h"
#include "keyboardmanager/common/KeyboardManagerState.h"
#include "ShortcutControl.h"

//Both static members are initialized to null
HWND SingleKeyRemapControl::EditKeyboardWindowHandle = nullptr;
KeyboardManagerState* SingleKeyRemapControl::keyboardManagerState = nullptr;
// Initialized as new vector
std::vector<std::pair<std::vector<std::variant<DWORD, Shortcut>>, std::wstring>> SingleKeyRemapControl::singleKeyRemapBuffer;

SingleKeyRemapControl::SingleKeyRemapControl(Grid table, const int colIndex)
{
    typeKey = Button();
    typeKey.as<Button>().Width(KeyboardManagerConstants::RemapTableDropDownWidth);
    typeKey.as<Button>().Content(winrt::box_value(L"Type"));

    singleKeyRemapControlLayout = StackPanel();
    singleKeyRemapControlLayout.as<StackPanel>().Margin({ 0, 0, 0, 10 });
    singleKeyRemapControlLayout.as<StackPanel>().Spacing(10);
    singleKeyRemapControlLayout.as<StackPanel>().Children().Append(typeKey.as<Button>());

    // Key column
    if (colIndex == 0)
    {
        keyDropDownControlObjects.push_back(std::move(std::unique_ptr<KeyDropDownControl>(new KeyDropDownControl(false))));
        singleKeyRemapControlLayout.as<StackPanel>().Children().Append(keyDropDownControlObjects[0]->GetComboBox());
        // Set selection handler for the drop down
        keyDropDownControlObjects[0]->SetSelectionHandler(table, singleKeyRemapControlLayout.as<StackPanel>(), colIndex, singleKeyRemapBuffer);
    }

    // Hybrid column
    else
    {
        hybridDropDownStackPanel = StackPanel();
        hybridDropDownStackPanel.as<StackPanel>().Spacing(KeyboardManagerConstants::ShortcutTableDropDownSpacing);
        hybridDropDownStackPanel.as<StackPanel>().Orientation(Windows::UI::Xaml::Controls::Orientation::Horizontal);
        KeyDropDownControl::AddDropDown(table, singleKeyRemapControlLayout.as<StackPanel>(), hybridDropDownStackPanel.as<StackPanel>(), colIndex, singleKeyRemapBuffer, keyDropDownControlObjects, nullptr, true, true);
        singleKeyRemapControlLayout.as<StackPanel>().Children().Append(hybridDropDownStackPanel.as<StackPanel>());
    }

    StackPanel controlStackPanel = singleKeyRemapControlLayout.as<StackPanel>();
    typeKey.as<Button>().Click([&, table, colIndex, controlStackPanel](winrt::Windows::Foundation::IInspectable const& sender, RoutedEventArgs const&) {
        // Using the XamlRoot of the typeKey to get the root of the XAML host
        if (colIndex == 0)
        {
            keyboardManagerState->SetUIState(KeyboardManagerUIState::DetectSingleKeyRemapWindowActivated, EditKeyboardWindowHandle);
            createDetectKeyWindow(sender, sender.as<Button>().XamlRoot(), *keyboardManagerState);
        }
        else
        {
            keyboardManagerState->SetUIState(KeyboardManagerUIState::DetectShortcutWindowInEditKeyboardWindowActivated, EditKeyboardWindowHandle);
            ShortcutControl::createDetectShortcutWindow(sender, sender.as<Button>().XamlRoot(), *keyboardManagerState, colIndex, table, keyDropDownControlObjects, controlStackPanel, nullptr, true, true, EditKeyboardWindowHandle, singleKeyRemapBuffer);
        }
    });

    singleKeyRemapControlLayout.as<StackPanel>().UpdateLayout();
}

// Function to add a new row to the remap keys table. If the originalKey and newKey args are provided, then the displayed remap keys are set to those values.
void SingleKeyRemapControl::AddNewControlKeyRemapRow(Grid& parent, std::vector<std::vector<std::unique_ptr<SingleKeyRemapControl>>>& keyboardRemapControlObjects, const DWORD originalKey, const std::variant<DWORD, Shortcut> newKey)
{
    // Create new SingleKeyRemapControl objects dynamically so that we does not get destructed
    std::vector<std::unique_ptr<SingleKeyRemapControl>> newrow;
    newrow.push_back(std::move(std::unique_ptr<SingleKeyRemapControl>(new SingleKeyRemapControl(parent, 0))));
    newrow.push_back(std::move(std::unique_ptr<SingleKeyRemapControl>(new SingleKeyRemapControl(parent, 1))));
    keyboardRemapControlObjects.push_back(std::move(newrow));

    // Add to grid
    parent.RowDefinitions().Append(RowDefinition());
    parent.SetColumn(keyboardRemapControlObjects[keyboardRemapControlObjects.size() - 1][0]->getSingleKeyRemapControl(), KeyboardManagerConstants::RemapTableOriginalColIndex);
    parent.SetRow(keyboardRemapControlObjects[keyboardRemapControlObjects.size() - 1][0]->getSingleKeyRemapControl(), parent.RowDefinitions().Size() - 1);
    parent.SetColumn(keyboardRemapControlObjects[keyboardRemapControlObjects.size() - 1][1]->getSingleKeyRemapControl(), KeyboardManagerConstants::RemapTableNewColIndex);
    parent.SetRow(keyboardRemapControlObjects[keyboardRemapControlObjects.size() - 1][1]->getSingleKeyRemapControl(), parent.RowDefinitions().Size() - 1);
    // SingleKeyRemapControl for the original key.
    parent.Children().Append(keyboardRemapControlObjects[keyboardRemapControlObjects.size() - 1][0]->getSingleKeyRemapControl());

    // Arrow icon
    FontIcon arrowIcon;
    arrowIcon.FontFamily(Media::FontFamily(L"Segoe MDL2 Assets"));
    arrowIcon.Glyph(L"\xE72A");
    arrowIcon.VerticalAlignment(VerticalAlignment::Center);
    arrowIcon.HorizontalAlignment(HorizontalAlignment::Center);
    parent.SetColumn(arrowIcon, KeyboardManagerConstants::RemapTableArrowColIndex);
    parent.SetRow(arrowIcon, parent.RowDefinitions().Size() - 1);
    parent.Children().Append(arrowIcon);

    // SingleKeyRemapControl for the new remap key
    parent.Children().Append(keyboardRemapControlObjects[keyboardRemapControlObjects.size() - 1][1]->getSingleKeyRemapControl());

    // Set the key text if the two keys are not null (i.e. default args)
    if (originalKey != NULL && !(newKey.index() == 0 && std::get<DWORD>(newKey) == NULL) && !(newKey.index() == 1 && !std::get<Shortcut>(newKey).IsValidShortcut()))
    {
        singleKeyRemapBuffer.push_back(std::make_pair<std::vector<std::variant<DWORD, Shortcut>>, std::wstring>(std::vector<std::variant<DWORD, Shortcut>>{ originalKey, newKey }, L""));
        std::vector<DWORD> keyCodes = keyboardManagerState->keyboardMap.GetKeyCodeList();
        std::vector<DWORD> shortcutListKeyCodes = keyboardManagerState->keyboardMap.GetKeyCodeList(true);
        auto it = std::find(keyCodes.begin(), keyCodes.end(), originalKey);
        if (it != keyCodes.end())
        {
            keyboardRemapControlObjects[keyboardRemapControlObjects.size() - 1][0]->keyDropDownControlObjects[0]->SetSelectedIndex((int32_t)std::distance(keyCodes.begin(), it));
        }
        if (newKey.index() == 0)
        {
            it = std::find(shortcutListKeyCodes.begin(), shortcutListKeyCodes.end(), std::get<DWORD>(newKey));
            if (it != shortcutListKeyCodes.end())
            {
                keyboardRemapControlObjects[keyboardRemapControlObjects.size() - 1][1]->keyDropDownControlObjects[0]->SetSelectedIndex((int32_t)std::distance(shortcutListKeyCodes.begin(), it));
            }
        }
        else
        {
            KeyDropDownControl::AddShortcutToControl(std::get<Shortcut>(newKey), parent, keyboardRemapControlObjects[keyboardRemapControlObjects.size() - 1][1]->hybridDropDownStackPanel.as<StackPanel>(), *keyboardManagerState, 1, keyboardRemapControlObjects[keyboardRemapControlObjects.size() - 1][1]->keyDropDownControlObjects, singleKeyRemapBuffer, keyboardRemapControlObjects[keyboardRemapControlObjects.size() - 1][1]->singleKeyRemapControlLayout.as<StackPanel>(), nullptr, true, true);
        }
    }
    else
    {
        // Initialize both keys to NULL
        singleKeyRemapBuffer.push_back(std::make_pair<std::vector<std::variant<DWORD, Shortcut>>, std::wstring>(std::vector<std::variant<DWORD, Shortcut>>{ NULL, NULL }, L""));
    }

    // Delete row button
    Windows::UI::Xaml::Controls::Button deleteRemapKeys;
    FontIcon deleteSymbol;
    deleteSymbol.FontFamily(Media::FontFamily(L"Segoe MDL2 Assets"));
    deleteSymbol.Glyph(L"\xE74D");
    deleteRemapKeys.Content(deleteSymbol);
    deleteRemapKeys.Background(Media::SolidColorBrush(Colors::Transparent()));
    deleteRemapKeys.HorizontalAlignment(HorizontalAlignment::Center);
    deleteRemapKeys.Click([&](winrt::Windows::Foundation::IInspectable const& sender, RoutedEventArgs const&) {
        Button currentButton = sender.as<Button>();
        uint32_t index;
        // Get index of delete button
        UIElementCollection children = parent.Children();
        children.IndexOf(currentButton, index);
        uint32_t lastIndexInRow = index + ((KeyboardManagerConstants::RemapTableColCount - 1) - KeyboardManagerConstants::RemapTableRemoveColIndex);
        // Change the row index of elements appearing after the current row, as we will delete the row definition
        for (uint32_t i = lastIndexInRow + 1; i < children.Size(); i++)
        {
            int32_t elementRowIndex = parent.GetRow(children.GetAt(i).as<FrameworkElement>());
            parent.SetRow(children.GetAt(i).as<FrameworkElement>(), elementRowIndex - 1);
        }

        for (int i = 0; i < KeyboardManagerConstants::RemapTableColCount; i++)
        {
            parent.Children().RemoveAt(lastIndexInRow - i);
        }

        // Calculate row index in the buffer from the grid child index (first two children are header elements and then three children in each row)
        int bufferIndex = (lastIndexInRow - KeyboardManagerConstants::RemapTableHeaderCount) / KeyboardManagerConstants::RemapTableColCount;
        // Delete the row definition
        parent.RowDefinitions().RemoveAt(bufferIndex + 1);
        // delete the row from the buffer.
        singleKeyRemapBuffer.erase(singleKeyRemapBuffer.begin() + bufferIndex);
        // delete the SingleKeyRemapControl objects so that they get destructed
        keyboardRemapControlObjects.erase(keyboardRemapControlObjects.begin() + bufferIndex);
    });
    parent.SetColumn(deleteRemapKeys, KeyboardManagerConstants::RemapTableRemoveColIndex);
    parent.SetRow(deleteRemapKeys, parent.RowDefinitions().Size() - 1);
    parent.Children().Append(deleteRemapKeys);
    parent.UpdateLayout();
}

// Function to return the stack panel element of the SingleKeyRemapControl. This is the externally visible UI element which can be used to add it to other layouts
StackPanel SingleKeyRemapControl::getSingleKeyRemapControl()
{
    return singleKeyRemapControlLayout.as<StackPanel>();
}

// Function to create the detect remap key UI window
void SingleKeyRemapControl::createDetectKeyWindow(winrt::Windows::Foundation::IInspectable const& sender, XamlRoot xamlRoot, KeyboardManagerState& keyboardManagerState)
{
    // ContentDialog for detecting remap key. This is the parent UI element.
    ContentDialog detectRemapKeyBox;

    // ContentDialog requires manually setting the XamlRoot (https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.contentdialog#contentdialog-in-appwindow-or-xaml-islands)
    detectRemapKeyBox.XamlRoot(xamlRoot);
    detectRemapKeyBox.Title(box_value(L"Press a key on selected keyboard:"));
    detectRemapKeyBox.IsPrimaryButtonEnabled(false);
    detectRemapKeyBox.IsSecondaryButtonEnabled(false);

    // Get the linked text block for the "Type Key" button that was clicked
    ComboBox linkedRemapDropDown = KeyboardManagerHelper::getSiblingElement(sender).as<ComboBox>();

    auto unregisterKeys = [&keyboardManagerState]() {
        std::thread t1(&KeyboardManagerState::UnregisterKeyDelay, &keyboardManagerState, VK_ESCAPE);
        std::thread t2(&KeyboardManagerState::UnregisterKeyDelay, &keyboardManagerState, VK_RETURN);
        t1.detach();
        t2.detach();
    };

    auto onPressEnter = [linkedRemapDropDown,
                         detectRemapKeyBox,
                         &keyboardManagerState,
                         unregisterKeys] {
        // Save the detected key in the linked text block
        DWORD detectedKey = keyboardManagerState.GetDetectedSingleRemapKey();

        if (detectedKey != NULL)
        {
            std::vector<DWORD> keyCodeList = keyboardManagerState.keyboardMap.GetKeyCodeList();
            // Update the drop down list with the new language to ensure that the correct key is displayed
            linkedRemapDropDown.ItemsSource(KeyboardManagerHelper::ToBoxValue(keyboardManagerState.keyboardMap.GetKeyNameList()));
            auto it = std::find(keyCodeList.begin(), keyCodeList.end(), detectedKey);
            if (it != keyCodeList.end())
            {
                linkedRemapDropDown.SelectedIndex((int32_t)std::distance(keyCodeList.begin(), it));
            }
        }
        // Hide the type key UI
        detectRemapKeyBox.Hide();
    };

    auto onReleaseEnter = [&keyboardManagerState,
                           unregisterKeys] {
        // Reset the keyboard manager UI state
        keyboardManagerState.ResetUIState();
        // Revert UI state back to Edit Keyboard window
        keyboardManagerState.SetUIState(KeyboardManagerUIState::EditKeyboardWindowActivated, EditKeyboardWindowHandle);
        unregisterKeys();
    };

    auto onAccept = [onPressEnter,
                     onReleaseEnter] {
        onPressEnter();
        onReleaseEnter();
    };

    TextBlock primaryButtonText;
    primaryButtonText.Text(L"OK");

    Button primaryButton;
    primaryButton.HorizontalAlignment(HorizontalAlignment::Stretch);
    primaryButton.Margin({ 2, 2, 2, 2 });
    primaryButton.Content(primaryButtonText);
    primaryButton.Click([onAccept](winrt::Windows::Foundation::IInspectable const& sender, RoutedEventArgs const&) {
        onAccept();
    });

    keyboardManagerState.RegisterKeyDelay(
        VK_RETURN,
        std::bind(&KeyboardManagerState::SelectDetectedRemapKey, &keyboardManagerState, std::placeholders::_1),
        [primaryButton, onPressEnter, detectRemapKeyBox](DWORD) {
            detectRemapKeyBox.Dispatcher().RunAsync(
                Windows::UI::Core::CoreDispatcherPriority::Normal,
                [primaryButton, onPressEnter] {
                    // Use the base medium low brush to be consistent with the theme
                    primaryButton.Background(Windows::UI::Xaml::Application::Current().Resources().Lookup(box_value(L"SystemControlBackgroundBaseMediumLowBrush")).as<Windows::UI::Xaml::Media::SolidColorBrush>());
                    onPressEnter();
                });
        },
        [onReleaseEnter](DWORD) {
            onReleaseEnter();
        });

    TextBlock cancelButtonText;
    cancelButtonText.Text(L"Cancel");

    Button cancelButton;
    cancelButton.HorizontalAlignment(HorizontalAlignment::Stretch);
    cancelButton.Margin({ 2, 2, 2, 2 });
    cancelButton.Content(cancelButtonText);
    // Cancel button
    cancelButton.Click([detectRemapKeyBox, unregisterKeys, &keyboardManagerState](winrt::Windows::Foundation::IInspectable const& sender, RoutedEventArgs const&) {
        // Reset the keyboard manager UI state
        keyboardManagerState.ResetUIState();
        // Revert UI state back to Edit Keyboard window
        keyboardManagerState.SetUIState(KeyboardManagerUIState::EditKeyboardWindowActivated, EditKeyboardWindowHandle);
        unregisterKeys();
        detectRemapKeyBox.Hide();
    });

    keyboardManagerState.RegisterKeyDelay(
        VK_ESCAPE,
        std::bind(&KeyboardManagerState::SelectDetectedRemapKey, &keyboardManagerState, std::placeholders::_1),
        [&keyboardManagerState, detectRemapKeyBox, unregisterKeys](DWORD) {
            detectRemapKeyBox.Dispatcher().RunAsync(
                Windows::UI::Core::CoreDispatcherPriority::Normal,
                [detectRemapKeyBox] {
                    detectRemapKeyBox.Hide();
                });

            keyboardManagerState.ResetUIState();
            // Revert UI state back to Edit Keyboard window
            keyboardManagerState.SetUIState(KeyboardManagerUIState::EditKeyboardWindowActivated, EditKeyboardWindowHandle);
            unregisterKeys();
        },
        nullptr);

    // StackPanel parent for the displayed text in the dialog
    Windows::UI::Xaml::Controls::StackPanel stackPanel;
    detectRemapKeyBox.Content(stackPanel);

    // Header textblock
    TextBlock text;
    text.Text(L"Key Pressed:");
    text.Margin({ 0, 0, 0, 10 });
    stackPanel.Children().Append(text);

    // Target StackPanel to place the selected key
    Windows::UI::Xaml::Controls::StackPanel keyStackPanel;
    keyStackPanel.Orientation(Orientation::Horizontal);
    stackPanel.Children().Append(keyStackPanel);

    TextBlock holdEscInfo;
    holdEscInfo.Text(L"Hold Esc to discard");
    holdEscInfo.FontSize(12);
    holdEscInfo.Margin({ 0, 20, 0, 0 });
    stackPanel.Children().Append(holdEscInfo);

    TextBlock holdEnterInfo;
    holdEnterInfo.Text(L"Hold Enter to continue");
    holdEnterInfo.FontSize(12);
    holdEnterInfo.Margin({ 0, 0, 0, 0 });
    stackPanel.Children().Append(holdEnterInfo);

    ColumnDefinition primaryButtonColumn;
    ColumnDefinition cancelButtonColumn;

    Grid buttonPanel;
    buttonPanel.Margin({ 0, 20, 0, 0 });
    buttonPanel.HorizontalAlignment(HorizontalAlignment::Stretch);
    buttonPanel.ColumnDefinitions().Append(primaryButtonColumn);
    buttonPanel.ColumnDefinitions().Append(cancelButtonColumn);
    buttonPanel.SetColumn(primaryButton, 0);
    buttonPanel.SetColumn(cancelButton, 1);

    buttonPanel.Children().Append(primaryButton);
    buttonPanel.Children().Append(cancelButton);

    stackPanel.Children().Append(buttonPanel);
    stackPanel.UpdateLayout();

    // Configure the keyboardManagerState to store the UI information.
    keyboardManagerState.ConfigureDetectSingleKeyRemapUI(keyStackPanel);

    // Show the dialog
    detectRemapKeyBox.ShowAsync();
}