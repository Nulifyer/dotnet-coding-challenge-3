using System;

namespace sage.challenge.api; 

internal static partial class ExceptionExtensions
{
    public static SimpleError ToSimpleError(this Exception ex)
    {
        return new SimpleError(ex);
    }
}