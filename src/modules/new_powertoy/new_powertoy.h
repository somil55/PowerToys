#pragma once
#include <iostream>
#include <interface/powertoy_module_interface.h>

extern class NewPowertoy* instance;
class TargetState;
class UserWindow;

class NewPowertoy : public PowertoyModuleIface
{
public:
    enum class State
    {
        CREATED,
        SHOWN,
        HIDDEN,
        DESTROYED
    };
    State state;
    NewPowertoy();
    virtual const wchar_t* get_name() override;
    virtual const wchar_t** get_events() override;
    virtual bool get_config(wchar_t* buffer, int* buffer_size) override;
    virtual void set_config(const wchar_t* config) override;
    virtual void enable() override;
    virtual void disable() override;
    virtual bool is_enabled() override;
    virtual intptr_t signal_event(const wchar_t* name, intptr_t data) override;
    virtual void register_system_menu_helper(PowertoySystemMenuIface* helper) override {}
    virtual void signal_system_menu_action(const wchar_t* name) override {}

    virtual void create();
    virtual void show();
    void RunMessageLoop();
    virtual void hide();
    virtual void destroy();
    virtual void displayMessageBox(wchar_t* message);

private:
    bool m_enabled;
    std::unique_ptr<TargetState> target_state;
    std::unique_ptr<UserWindow> window;

    void init_settings();
};