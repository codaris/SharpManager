#pragma once
#ifndef __RESULT_H__
#define __RESULT_H__

enum ResultType
{
    Ok = 0,
    Timeout = 1,
    Cancelled = 2,
    Unexpected = 3,
    Overflow = 4,
    SyncError = 5,
    End = 0xFF
};

class Result
{
    public:
    Result(int v) : value(v) {}    
    Result(ResultType rt) : value(-rt) {}

    ~Result() = default;
    Result(const Result& other) = default;
    Result& operator=(const Result& other) = default;
    Result(Result&& other) noexcept = default;
    Result& operator=(Result&& other) noexcept = default;

    // Type conversion operator
    operator int() const { return Value(); }
    operator byte() const { return Value(); }
    operator ResultType() const { return Type(); }
    bool HasValue() const { return value >= 0; }
    bool IsError() const { return value < 0; }
    bool IsDone() const { return value == (-ResultType::End); }
    ResultType Type() const {  return (value >= 0 ? ResultType::Ok : (ResultType)-value); }
    int Value() const { return value >= 0 ? value : 0; }
    Result AsErrorCode() { return HasValue() ? (ResultType)Value() : Type(); }

    private:
    int value;
};

#endif