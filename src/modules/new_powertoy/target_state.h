#pragma once
#include <deque>
#include <thread>
#include <mutex>
#include <condition_variable>
#include <chrono>

struct KeyEvent
{
    unsigned vk_code;
};

class TargetState
{
public:
    TargetState();
    void signal_event(unsigned vk_code);
    void exit();

private:
    KeyEvent next();
    void thread_proc();
    std::thread thread;
    std::mutex mutex;
    std::condition_variable cv;
    std::deque<KeyEvent> events;
};