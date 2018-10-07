using System;

namespace Kampus.WordSearcher
{
    public class Result<T>
    {
        public readonly Status Status;
        public readonly T Value;

        bool IsSuccess
        {
            get { return Status == Status.Success; }
        }

        public bool IsFaulted
        {
            get { return !IsSuccess; }
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(Status.Success, value);
        }

        public static Result<T> Fail(Status status)
        {
            if (status != Status.Success)
                return new Result<T>(status, default(T));
            throw new ArgumentException();
        }

        public Result<TR> Select<TR>(Func<T, TR> selector)
        {
            return IsSuccess ? Result<TR>.Success(selector(Value)) : Result<TR>.Fail(Status);
        }

        Result(Status status, T value)
        {
            Status = status;
            Value = value;
        }
    }
}