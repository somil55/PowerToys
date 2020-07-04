#include "pch.h"
#include "CppUnitTest.h"
#include "MockedInput.h"
#include <keyboardmanager/common/KeyboardManagerState.h>
#include <keyboardmanager/dll/KeyboardEventHandlers.h>
#include "TestHelpers.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace RemappingLogicTests
{
    // Tests for single key remapping logic
    TEST_CLASS (SingleKeyRemappingTests)
    {
    private:
        MockedInput mockedInputHandler;
        KeyboardManagerState testState;

    public:
        TEST_METHOD_INITIALIZE(InitializeTestEnv)
        {
            // Reset test environment
            TestHelpers::ResetTestEnv(mockedInputHandler, testState);

            // Set HandleSingleKeyRemapEvent as the hook procedure
            std::function<intptr_t(LowlevelKeyboardEvent*)> currentHookProc = std::bind(&KeyboardEventHandlers::HandleSingleKeyRemapEvent, std::ref(mockedInputHandler), std::placeholders::_1, std::ref(testState));
            mockedInputHandler.SetHookProc(currentHookProc);
        }

        // Test if correct keyboard states are set for a single key remap
        TEST_METHOD (RemappedKey_ShouldSetTargetKeyState_OnKeyEvent)
        {
            // Remap A to B
            testState.AddSingleKeyRemap(0x41, 0x42);
            const int nInputs = 1;

            INPUT input[nInputs] = {};
            input[0].type = INPUT_KEYBOARD;
            input[0].ki.wVk = 0x41;

            // Send A keydown
            mockedInputHandler.SendVirtualInput(1, input, sizeof(INPUT));

            // A key state should be unchanged, and B key state should be true
            Assert::AreEqual(mockedInputHandler.GetVirtualKeyState(0x41), false);
            Assert::AreEqual(mockedInputHandler.GetVirtualKeyState(0x42), true);
            input[0].ki.dwFlags = KEYEVENTF_KEYUP;

            // Send A keyup
            mockedInputHandler.SendVirtualInput(1, input, sizeof(INPUT));

            // A key state should be unchanged, and B key state should be false
            Assert::AreEqual(mockedInputHandler.GetVirtualKeyState(0x41), false);
            Assert::AreEqual(mockedInputHandler.GetVirtualKeyState(0x42), false);
        }

        // Test if key is suppressed if a key is disabled by single key remap
        TEST_METHOD (RemappedKeyDisabled_ShouldNotChangeKeyState_OnKeyEvent)
        {
            // Remap A to 0x0 (disabled)
            testState.AddSingleKeyRemap(0x41, 0x0);
            const int nInputs = 1;

            INPUT input[nInputs] = {};
            input[0].type = INPUT_KEYBOARD;
            input[0].ki.wVk = 0x41;

            // Send A keydown
            mockedInputHandler.SendVirtualInput(1, input, sizeof(INPUT));

            // A key state should be unchanged
            Assert::AreEqual(mockedInputHandler.GetVirtualKeyState(0x41), false);
            input[0].ki.dwFlags = KEYEVENTF_KEYUP;

            // Send A keyup
            mockedInputHandler.SendVirtualInput(1, input, sizeof(INPUT));

            // A key state should be unchanged
            Assert::AreEqual(mockedInputHandler.GetVirtualKeyState(0x41), false);
        }

        // Test if correct keyboard states are set for a remap to Win (Both) key
        TEST_METHOD (RemappedKeyToWinBoth_ShouldSetWinLeftKeyState_OnKeyEvent)
        {
            // Remap A to Common Win key
            testState.AddSingleKeyRemap(0x41, CommonSharedConstants::VK_WIN_BOTH);
            const int nInputs = 1;

            INPUT input[nInputs] = {};
            input[0].type = INPUT_KEYBOARD;
            input[0].ki.wVk = 0x41;

            // Send A keydown
            mockedInputHandler.SendVirtualInput(1, input, sizeof(INPUT));

            // A key state should be unchanged, and common Win key state should be true
            Assert::AreEqual(mockedInputHandler.GetVirtualKeyState(0x41), false);
            Assert::AreEqual(mockedInputHandler.GetVirtualKeyState(VK_LWIN), true);
            input[0].ki.dwFlags = KEYEVENTF_KEYUP;

            // Send A keyup
            mockedInputHandler.SendVirtualInput(1, input, sizeof(INPUT));

            // A key state should be unchanged, and common Win key state should be false
            Assert::AreEqual(mockedInputHandler.GetVirtualKeyState(0x41), false);
            Assert::AreEqual(mockedInputHandler.GetVirtualKeyState(VK_LWIN), false);
        }

        // Test if SendVirtualInput is sent exactly once with the suppress flag when Caps Lock is remapped to Ctrl
        TEST_METHOD (HandleSingleKeyRemapEvent_ShouldSendVirutalInputWithSuppressFlagExactlyOnce_WhenCapsLockIsMappedToCtrlAltShift)
        {
            // Set sendvirtualinput call count condition to return true if the key event was sent with the suppress flag
            mockedInputHandler.SetSendVirtualInputTestHandler([](LowlevelKeyboardEvent* data) {
                if (data->lParam->dwExtraInfo == KeyboardManagerConstants::KEYBOARDMANAGER_SUPPRESS_FLAG)
                    return true;
                else
                    return false;
            });

            // Remap Caps Lock to Ctrl
            testState.AddSingleKeyRemap(VK_CAPITAL, VK_CONTROL);
            const int nInputs = 1;

            INPUT input[nInputs] = {};
            input[0].type = INPUT_KEYBOARD;
            input[0].ki.wVk = VK_CAPITAL;

            // Send Caps Lock keydown
            mockedInputHandler.SendVirtualInput(1, input, sizeof(INPUT));

            // SendVirtualInput should be called exactly once with the above condition
            Assert::AreEqual(1, mockedInputHandler.GetSendVirtualInputCallCount());
        }

        // Test if SendVirtualInput is sent exactly once with the suppress flag when Ctrl is remapped to Caps Lock
        TEST_METHOD (HandleSingleKeyRemapEvent_ShouldSendVirutalInputWithSuppressFlagExactlyOnce_WhenCtrlAltShiftIsMappedToCapsLock)
        {
            // Set sendvirtualinput call count condition to return true if the key event was sent with the suppress flag
            mockedInputHandler.SetSendVirtualInputTestHandler([](LowlevelKeyboardEvent* data) {
                if (data->lParam->dwExtraInfo == KeyboardManagerConstants::KEYBOARDMANAGER_SUPPRESS_FLAG)
                    return true;
                else
                    return false;
            });

            // Remap Ctrl to Caps Lock
            testState.AddSingleKeyRemap(VK_CONTROL, VK_CAPITAL);
            const int nInputs = 1;

            INPUT input[nInputs] = {};
            input[0].type = INPUT_KEYBOARD;
            input[0].ki.wVk = VK_CONTROL;

            // Send Ctrl keydown
            mockedInputHandler.SendVirtualInput(1, input, sizeof(INPUT));

            // SendVirtualInput should be called exactly once with the above condition
            Assert::AreEqual(1, mockedInputHandler.GetSendVirtualInputCallCount());
        }
    };
}
