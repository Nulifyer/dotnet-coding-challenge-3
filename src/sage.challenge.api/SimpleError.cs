using System;

namespace sage.challenge.api;

public class SimpleError
{
    public string Error { get; set; }

    public SimpleError(Exception ex)
    {
        Error = ex?.Message;
    }
    public SimpleError(string error)
    {
        Error = error;
    }
}