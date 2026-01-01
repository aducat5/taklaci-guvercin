using System;
using System.Collections.Generic;

namespace TaklaciGuvercin.Shared.Common
{
    [Serializable]
    public class Result
    {
        public bool IsSuccess;
        public bool IsFailure => !IsSuccess;
        public string Error = string.Empty;
        public List<string> Errors = new List<string>();
    }

    [Serializable]
    public class Result<T> : Result
    {
        public T Value;
    }
}
