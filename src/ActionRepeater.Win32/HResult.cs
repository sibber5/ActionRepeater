namespace ActionRepeater.Win32;

public enum HResult : uint
{
    /// <summary>
    /// Operation successful.
    /// </summary>
    S_OK = 0x00000000u,

    /// <summary>
    /// Operation successful. (negative condition/no operation)
    /// </summary>
    S_FALSE = 0x00000001u,

    /// <summary>
    /// Not implemented.
    /// </summary>
    E_NOTIMPL = 0x80004001u,

    /// <summary>
    /// No such interface supported.
    /// </summary>
    E_NOINTERFACE = 0x80004002u,

    /// <summary>
    /// Pointer that is not valid.
    /// </summary>
    E_POINTER = 0x80004003u,

    /// <summary>
    /// Operation aborted.
    /// </summary>
    E_ABORT = 0x80004004u,

    /// <summary>
    /// Unspecified failure.
    /// </summary>
    E_FAIL = 0x80004005u,

    /// <summary>
    /// Unexpected failure.
    /// </summary>
    E_UNEXPECTED = 0x8000FFFFu,

    /// <summary>
    /// General access denied error.
    /// </summary>
    E_ACCESSDENIED = 0x80070005u,

    /// <summary>
    /// Handle that is not valid.
    /// </summary>
    E_HANDLE = 0x80070006u,

    /// <summary>
    /// Failed to allocate necessary memory.
    /// </summary>
    E_OUTOFMEMORY = 0x8007000Eu,

    /// <summary>
    /// One or more arguments are not valid.
    /// </summary>
    E_INVALIDARG = 0x80070057u,

    /// <summary>
    /// The operation was canceled by the user. (Error source 7 means Win32.)
    /// </summary>
    /// <SeeAlso href="https://learn.microsoft.com/windows/win32/debug/system-error-codes--1000-1299-"/>
    /// <SeeAlso href="https://en.wikipedia.org/wiki/HRESULT"/>
    E_CANCELLED = 0x800704C7u,
}
