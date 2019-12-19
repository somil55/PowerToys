#include "pch.h"
#include "target_state.h"
#include "new_powertoy.h"


TargetState::TargetState() :
    thread(&TargetState::thread_proc, this)
{
}

void TargetState::signal_event(unsigned vk_code)
{
    std::unique_lock lock(mutex);
    events.push_back({ vk_code });
    lock.unlock();
    cv.notify_one();
}

KeyEvent TargetState::next()
{
    auto e = events.front();
    events.pop_front();
    return e;
}

void TargetState::exit()
{
    std::unique_lock lock(mutex);
    events.clear();
    lock.unlock();
    cv.notify_one();
    thread.join();
}

void TargetState::thread_proc()
{
    while (true)
    {
        std::unique_lock lock(mutex);
        if (events.empty())
        {
            cv.wait(lock);
        }
        if (instance->state == NewPowertoy::State::DESTROYED)
            return;
        auto event = next();
        if (event.vk_code == 0x44)
        {
            MessageBox(NULL, L"OpenedWindows", L"Close", MB_OK);
        }
        lock.unlock();
    }
}
