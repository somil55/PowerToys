#include "pch.h"
#include "Helpers.h"
#include <regex>
#include <ShlGuid.h>
#include <cstring>
#include <filesystem>

namespace fs = std::filesystem;

HRESULT GetTrimmedFileName(_Out_ PWSTR result, UINT cchMax, _In_ PCWSTR source)
{
    HRESULT hr = (source && wcslen(source) > 0) ? S_OK : E_INVALIDARG;     
    if (SUCCEEDED(hr))
    {
        PWSTR newName = nullptr;
        hr = SHStrDup(source, &newName);
        if (SUCCEEDED(hr))
        {
            size_t firstValidIndex = 0, lastValidIndex = wcslen(newName) - 1;
            while (firstValidIndex <= lastValidIndex && iswspace(newName[firstValidIndex]))
            {
                firstValidIndex++;
            }
            while (firstValidIndex <= lastValidIndex && (iswspace(newName[lastValidIndex]) || newName[lastValidIndex] == L'.'))
            {
                lastValidIndex--;
            }
            newName[lastValidIndex + 1] = '\0';

            hr = StringCchCopy(result, cchMax, newName + firstValidIndex);
        }
        CoTaskMemFree(newName);
    }

    return hr;
}

HRESULT GetTransformedFileName(_Out_ PWSTR result, UINT cchMax, _In_ PCWSTR source, DWORD flags)
{
    std::locale::global(std::locale(""));
    HRESULT hr = (source && wcslen(source) > 0 && flags) ? S_OK : E_INVALIDARG;
    if (SUCCEEDED(hr))
    {
        if (flags & Uppercase)
        {
            if (flags & NameOnly)
            {
                std::wstring stem = fs::path(source).stem().wstring();
                std::transform(stem.begin(), stem.end(), stem.begin(), ::towupper);
                hr = StringCchPrintf(result, cchMax, L"%s%s", stem.c_str(), fs::path(source).extension().c_str());
            }
            else if (flags & ExtensionOnly)
            {
                std::wstring extension = fs::path(source).extension().wstring();
                if (!extension.empty())
                {
                    std::transform(extension.begin(), extension.end(), extension.begin(), ::towupper);
                    hr = StringCchPrintf(result, cchMax, L"%s%s", fs::path(source).stem().c_str(), extension.c_str());
                }
                else
                {
                    hr = StringCchCopy(result, cchMax, source);
                    if (SUCCEEDED(hr))
                    {
                        std::transform(result, result + wcslen(result), result, ::towupper);
                    }
                }
            }
            else
            {
                hr = StringCchCopy(result, cchMax, source);
                if (SUCCEEDED(hr))
                {
                    std::transform(result, result + wcslen(result), result, ::towupper);
                }
            }
        }
        else if (flags & Lowercase)
        {
            if (flags & NameOnly)
            {
                std::wstring stem = fs::path(source).stem().wstring();
                std::transform(stem.begin(), stem.end(), stem.begin(), ::towlower);
                hr = StringCchPrintf(result, cchMax, L"%s%s", stem.c_str(), fs::path(source).extension().c_str());
            }
            else if (flags & ExtensionOnly)
            {
                std::wstring extension = fs::path(source).extension().wstring();
                if (!extension.empty())
                {
                    std::transform(extension.begin(), extension.end(), extension.begin(), ::towlower);
                    hr = StringCchPrintf(result, cchMax, L"%s%s", fs::path(source).stem().c_str(), extension.c_str());
                }
                else
                {
                    hr = StringCchCopy(result, cchMax, source);
                    if (SUCCEEDED(hr))
                    {
                        std::transform(result, result + wcslen(result), result, ::towlower);
                    }
                }
            }
            else
            {
                hr = StringCchCopy(result, cchMax, source);
                if (SUCCEEDED(hr))
                {
                    std::transform(result, result + wcslen(result), result, ::towlower);
                }
            }
        }
        else if (flags & Titlecase)
        {
            if (!(flags & ExtensionOnly))
            {
                std::vector<std::wstring> exceptions = { L"a", L"an", L"to", L"the", L"at", L"by", L"for", L"in", L"of", L"on", L"up", L"and", L"as", L"but", L"or", L"nor" };
                std::wstring stem = fs::path(source).stem().wstring();
                std::wstring extension = fs::path(source).extension().wstring();

                size_t stemLength = stem.length();
                bool isFirstWord = true;

                while (stemLength > 0 && (iswspace(stem[stemLength - 1]) || iswpunct(stem[stemLength - 1])))
                {
                    stemLength--;
                }

                for (size_t i = 0; i < stemLength; i++)
                {
                    if (!i || iswspace(stem[i - 1]) || iswpunct(stem[i - 1]))
                    {
                        if (iswspace(stem[i]) || iswpunct(stem[i]))
                        {
                            continue;
                        }
                        size_t wordLength = 0;
                        while (i + wordLength < stemLength && !iswspace(stem[i + wordLength]) && !iswpunct(stem[i + wordLength]))
                        {
                            wordLength++;
                        }
                        if (isFirstWord || i + wordLength == stemLength || std::find(exceptions.begin(), exceptions.end(), stem.substr(i, wordLength)) == exceptions.end())
                        {
                            stem[i] = towupper(stem[i]);
                            isFirstWord = false;
                        }
                        else
                        {
                            stem[i] = towlower(stem[i]);
                        }
                    }
                    else
                    {
                        stem[i] = towlower(stem[i]);
                    }
                }
                hr = StringCchPrintf(result, cchMax, L"%s%s", stem.c_str(), extension.c_str());
            }
            else
            {
                hr = StringCchCopy(result, cchMax, source);
            }
        }
        else
        {
            hr = StringCchCopy(result, cchMax, source);
        }
    }

    return hr;
}

HRESULT GetDatedFileName(_Out_ PWSTR result, UINT cchMax, _In_ PCWSTR source, SYSTEMTIME LocalTime)
{
    HRESULT hr = (source && wcslen(source) > 0) ? S_OK : E_INVALIDARG;     
    if (SUCCEEDED(hr))
    {
        std::wregex pattern(L"\\$YYYY");
        std::wstring res(source);
        wchar_t replaceTerm[MAX_PATH] = {0};
        StringCchPrintf(replaceTerm, MAX_PATH, TEXT("%d"),LocalTime.wYear);
        res = regex_replace(res, pattern, replaceTerm);

        pattern = L"\\$SSS";
        StringCchPrintf(replaceTerm, MAX_PATH, TEXT("%03d"), LocalTime.wMilliseconds);
        res = regex_replace(res, pattern, replaceTerm);

        pattern = L"\\$MMM";
        StringCchPrintf(replaceTerm, MAX_PATH, TEXT("%03d"), LocalTime.wMilliseconds);
        res = regex_replace(res, pattern, replaceTerm);

        pattern = L"\\$mmm";
        StringCchPrintf(replaceTerm, MAX_PATH, TEXT("%03d"), LocalTime.wMilliseconds);
        res = regex_replace(res, pattern, replaceTerm);

        pattern = L"\\$fff";
        StringCchPrintf(replaceTerm, MAX_PATH, TEXT("%03d"), LocalTime.wMilliseconds);
        res = regex_replace(res, pattern, replaceTerm);

        pattern = L"\\$FFF";
        StringCchPrintf(replaceTerm, MAX_PATH, TEXT("%03d"), LocalTime.wMilliseconds);
        res = regex_replace(res, pattern, replaceTerm);

        pattern = L"\\$MM" ;
        StringCchPrintf(replaceTerm, MAX_PATH, TEXT("%02d"), LocalTime.wMonth);
        res = regex_replace(res, pattern, replaceTerm);

        pattern = L"\\$DD";
        StringCchPrintf(replaceTerm, MAX_PATH, TEXT("%02d"), LocalTime.wDay);
        res = regex_replace(res, pattern, replaceTerm);

        pattern = L"\\$hh";
        StringCchPrintf(replaceTerm, MAX_PATH, TEXT("%02d"), LocalTime.wHour);
        res = regex_replace(res, pattern, replaceTerm);

        pattern = L"\\$mm";
        StringCchPrintf(replaceTerm, MAX_PATH, TEXT("%02d"), LocalTime.wMinute);
        res = regex_replace(res, pattern, replaceTerm);

        pattern = L"\\$ss";
        StringCchPrintf(replaceTerm, MAX_PATH, TEXT("%02d"), LocalTime.wSecond);
        res = regex_replace(res, pattern, replaceTerm);

        hr = StringCchCopy(result, cchMax, res.c_str());
    }

    return hr;
}

HRESULT _ParseEnumItems(_In_ IEnumShellItems* pesi, _In_ IPowerRenameManager* psrm, _In_ int depth = 0)
{
    HRESULT hr = E_INVALIDARG;

    // We shouldn't get this deep since we only enum the contents of
    // regular folders but adding just in case
    if ((pesi) && (depth < (MAX_PATH / 2)))
    {
        hr = S_OK;

        ULONG celtFetched;
        CComPtr<IShellItem> spsi;
        while ((S_OK == pesi->Next(1, &spsi, &celtFetched)) && (SUCCEEDED(hr)))
        {
            CComPtr<IPowerRenameItemFactory> spsrif;
            hr = psrm->get_renameItemFactory(&spsrif);
            if (SUCCEEDED(hr))
            {
                CComPtr<IPowerRenameItem> spNewItem;
                hr = spsrif->Create(spsi, &spNewItem);
                if (SUCCEEDED(hr))
                {
                    spNewItem->put_depth(depth);
                    hr = psrm->AddItem(spNewItem);
                }

                if (SUCCEEDED(hr))
                {
                    bool isFolder = false;
                    if (SUCCEEDED(spNewItem->get_isFolder(&isFolder)) && isFolder)
                    {
                        // Bind to the IShellItem for the IEnumShellItems interface
                        CComPtr<IEnumShellItems> spesiNext;
                        hr = spsi->BindToHandler(nullptr, BHID_EnumItems, IID_PPV_ARGS(&spesiNext));
                        if (SUCCEEDED(hr))
                        {
                            // Parse the folder contents recursively
                            hr = _ParseEnumItems(spesiNext, psrm, depth + 1);
                        }
                    }
                }
            }

            spsi = nullptr;
        }
    }

    return hr;
}

// Iterate through the data source and add paths to the rotation manager
HRESULT EnumerateDataObject(_In_ IUnknown* dataSource, _In_ IPowerRenameManager* psrm)
{
    CComPtr<IShellItemArray> spsia;
    IDataObject* dataObj{};
    HRESULT hr;
    if (SUCCEEDED(dataSource->QueryInterface(IID_IDataObject, reinterpret_cast<void**>(&dataObj))))
    {
        hr = SHCreateShellItemArrayFromDataObject(dataObj, IID_PPV_ARGS(&spsia));
    }
    else
    {
        hr = dataSource->QueryInterface(IID_IShellItemArray, reinterpret_cast<void**>(&spsia));
    }
    if (SUCCEEDED(hr))
    {
        CComPtr<IEnumShellItems> spesi;
        hr = spsia->EnumItems(&spesi);
        if (SUCCEEDED(hr))
        {
            hr = _ParseEnumItems(spesi, psrm);
        }
    }

    return hr;
}

BOOL GetEnumeratedFileName(__out_ecount(cchMax) PWSTR pszUniqueName, UINT cchMax, __in PCWSTR pszTemplate, __in_opt PCWSTR pszDir, unsigned long ulMinLong, __inout unsigned long* pulNumUsed)
{
    PWSTR pszName = nullptr;
    HRESULT hr = S_OK;
    BOOL fRet = FALSE;
    int cchDir = 0;

    if (0 != cchMax && pszUniqueName)
    {
        *pszUniqueName = 0;
        if (pszDir)
        {
            hr = StringCchCopy(pszUniqueName, cchMax, pszDir);
            if (SUCCEEDED(hr))
            {
                hr = PathCchAddBackslashEx(pszUniqueName, cchMax, &pszName, nullptr);
                if (SUCCEEDED(hr))
                {
                    cchDir = lstrlen(pszDir);
                }
            }
        }
        else
        {
            cchDir = 0;
            pszName = pszUniqueName;
        }
    }
    else
    {
        hr = E_INVALIDARG;
    }

    int cchTmp = 0;
    int cchStem = 0;
    PCWSTR pszStem = nullptr;
    PCWSTR pszRest = nullptr;
    wchar_t szFormat[MAX_PATH] = { 0 };

    if (SUCCEEDED(hr))
    {
        pszStem = pszTemplate;

        pszRest = StrChr(pszTemplate, L'(');
        while (pszRest)
        {
            PCWSTR pszEndUniq = CharNext(pszRest);
            while (*pszEndUniq && *pszEndUniq >= L'0' && *pszEndUniq <= L'9')
            {
                pszEndUniq++;
            }

            if (*pszEndUniq == L')')
            {
                break;
            }

            pszRest = StrChr(CharNext(pszRest), L'(');
        }

        if (!pszRest)
        {
            pszRest = PathFindExtension(pszTemplate);
            cchStem = (int)(pszRest - pszTemplate);

            hr = StringCchCopy(szFormat, ARRAYSIZE(szFormat), L" (%lu)");
        }
        else
        {
            pszRest++;

            cchStem = (int)(pszRest - pszTemplate);

            while (*pszRest && *pszRest >= L'0' && *pszRest <= L'9')
            {
                pszRest++;
            }

            hr = StringCchCopy(szFormat, ARRAYSIZE(szFormat), L"%lu");
        }
    }

    unsigned long ulMax = 0;
    unsigned long ulMin = 0;
    if (SUCCEEDED(hr))
    {
        int cchFormat = lstrlen(szFormat);
        if (cchFormat < 3)
        {
            *pszUniqueName = L'\0';
            return FALSE;
        }
        ulMin = ulMinLong;
        cchTmp = cchMax - cchDir - cchStem - (cchFormat - 3);
        switch (cchTmp)
        {
        case 1:
            ulMax = 10;
            break;
        case 2:
            ulMax = 100;
            break;
        case 3:
            ulMax = 1000;
            break;
        case 4:
            ulMax = 10000;
            break;
        case 5:
            ulMax = 100000;
            break;
        default:
            if (cchTmp <= 0)
            {
                ulMax = ulMin;
            }
            else
            {
                ulMax = 1000000;
            }
            break;
        }
    }

    if (SUCCEEDED(hr))
    {
        hr = StringCchCopyN(pszName, pszUniqueName + cchMax - pszName, pszStem, cchStem);
        if (SUCCEEDED(hr))
        {
            PWSTR pszDigit = pszName + cchStem;

            for (unsigned long ul = ulMin; ((ul < ulMax) && (!fRet)); ul++)
            {
                wchar_t szTemp[MAX_PATH] = { 0 };
                hr = StringCchPrintf(szTemp, ARRAYSIZE(szTemp), szFormat, ul);
                if (SUCCEEDED(hr))
                {
                    hr = StringCchCat(szTemp, ARRAYSIZE(szTemp), pszRest);
                    if (SUCCEEDED(hr))
                    {
                        hr = StringCchCopy(pszDigit, pszUniqueName + cchMax - pszDigit, szTemp);
                        if (SUCCEEDED(hr))
                        {
                            if (!PathFileExists(pszUniqueName))
                            {
                                (*pulNumUsed) = ul;
                                fRet = TRUE;
                            }
                        }
                    }
                }
            }
        }
    }

    if (!fRet)
    {
        *pszUniqueName = L'\0';
    }

    return fRet;
}
