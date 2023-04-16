#pragma once
#ifndef __SHARPMANAGER_H__
#define __SHARPMANAGER_H__

namespace Command
{
    const int Ping = 1;
    const int DeviceSelect = 2;
    const int LoadTape = 3;
    const int Print = 4;
}

enum ErrorCode
{
    Unknown = 0,
    Timeout = 1,
    InvalidData = 2,
    Cancelled = 3,
    Unexpected = 4
};

#endif