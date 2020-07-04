#pragma once

#include <common.h>
#include <lib/PowerRenameInterfaces.h>

HRESULT GetTrimmedFileName(_Out_ PWSTR result, UINT cchMax, _In_ PCWSTR source);
HRESULT GetTransformedFileName(_Out_ PWSTR result, UINT cchMax, _In_ PCWSTR source, DWORD flags);
HRESULT EnumerateDataObject(_In_ IUnknown* pdo, _In_ IPowerRenameManager* psrm);
BOOL GetEnumeratedFileName(
    __out_ecount(cchMax) PWSTR pszUniqueName,
    UINT cchMax,
    __in PCWSTR pszTemplate,
    __in_opt PCWSTR pszDir,
    unsigned long ulMinLong,
    __inout unsigned long* pulNumUsed);