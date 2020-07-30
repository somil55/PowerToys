#pragma once

#include <string>
#include <string_view>
#include <vector>
#include <variant>
#include <optional>

namespace notifications
{
    constexpr inline const wchar_t TOAST_ACTIVATED_LAUNCH_ARG[] = L"-ToastActivated";

    void set_application_id(const std::wstring_view appID);
    void register_background_toast_handler();
    void run_desktop_app_activator_loop();

    bool register_application_id(const std::wstring_view appName, const std::wstring_view iconPath);
    void unregister_application_id();

    struct snooze_duration
    {
        std::wstring label;
        int minutes;
    };

    struct snooze_button
    {
        std::wstring snooze_title;
        std::vector<snooze_duration> durations;
    };

    struct link_button
    {
        std::wstring label;
        std::wstring url;
        bool context_menu = false;
    };

    struct background_activated_button
    {
        std::wstring label;
        bool context_menu = false;
    };

    struct progress_bar_params
    {
        std::wstring_view progress_title;
        float progress = 0.f;
    };

    struct toast_params
    {
        std::optional<std::wstring_view> tag;
        bool resend_if_scheduled = true;
        std::optional<progress_bar_params> progress_bar;
    };

    using action_t = std::variant<link_button, background_activated_button, snooze_button>;

    void show_toast(std::wstring plaintext_message, std::wstring title, toast_params params = {});
    void show_toast_with_activations(std::wstring plaintext_message, std::wstring title, std::wstring_view background_handler_id, std::vector<action_t> actions, toast_params params = {});
    void update_progress_bar_toast(std::wstring_view tag, progress_bar_params params);
}
