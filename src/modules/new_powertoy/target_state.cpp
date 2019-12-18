#include "pch.h"
#include "target_state.h"

TargetState::TargetState() : thread(&TargetState::thread_proc, this)
{
}

void TargetState::signal_event(unsigned vk_code)
{
    std::unique_lock lock(mutex);
    events.push_back({vk_code });
    lock.unlock();
    cv.notify_one();  
}

KeyEvent TargetState::next()
{
    auto e = events.front();
    events.pop_front();
    return e;
}

void TargetState::exit() {
    std::unique_lock lock(mutex);
    events.clear();
    lock.unlock();
    cv.notify_one();
    thread.join();
}

void TargetState::thread_proc()
{
    /*while (instance->state != instance->state::H)
    {
        
    }*/
}

