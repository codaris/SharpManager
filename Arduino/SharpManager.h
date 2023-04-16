#pragma once
#ifndef __SHARPMANAGER_H__
#define __SHARPMANAGER_H__

namespace Command
{
    const int Ping = 1;
    const int StartTape = 2;
    const int TapeHeaderBlock = 3;
    const int TapeDataBlock = 4;
    const int EndType = 5;
    const int LoadTape = 6;
}

enum ErrorCode
{
    Unknown = 0,
    BadPacketNum = 1,
    BadChecksum = 2,
    Overflow = 3,
    InvalidData = 4,
    Cancelled = 5,
    Unexpected = 6
};

#endif