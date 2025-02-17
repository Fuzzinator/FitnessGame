using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class StringExtensions
{
    private static string[] _escapeCharacters = new[] { "\'", "\n", "\r", "\t", "\b", "\f", "\\uxxxx"};
    private static string[] _illegalCharacters = new[] { ":", "*", "?", "\"", "<", ">", "|", "/", ".", "”", "\\", "+" };
    private static string[] _illegalFileCharacters = new[] { ":", "*", "?", "\"", "<", ">", "|", "/", "”", "\\", "+" };

    public static string CleanFileName(this string fileName)
    {
        if(fileName.ContainsIllegalFileCharacters())
        {
            return fileName.RemoveIllegalFileIOCharacters();
        }
        return fileName;
    }

    public static string RemoveIllegalIOCharacters(this string folderName)
    {
        var sb = new StringBuilder(folderName);
        foreach (var character in _escapeCharacters)
        {
            if (!folderName.Contains(character))
            {
                continue;
            }

            sb.Replace(character, string.Empty);
        }

        folderName = sb.ToString();
        foreach (var character in _illegalCharacters)
        {
            if (!folderName.Contains(character))
            {
                continue;
            }

            sb.Replace(character, string.Empty);
        }
        folderName = sb.ToString();
        return folderName;
    }
    
    public static string RemoveIllegalFileIOCharacters(this string folderName)
    {
        var sb = new StringBuilder(folderName);
        foreach (var character in _escapeCharacters)
        {
            if (!folderName.Contains(character))
            {
                continue;
            }

            sb.Replace(character, string.Empty);
        }

        folderName = sb.ToString();
        foreach (var character in _illegalFileCharacters)
        {
            if (!folderName.Contains(character))
            {
                continue;
            }

            sb.Replace(character, string.Empty);
        }
        folderName = sb.ToString();
        return folderName;
    }

    public static string RemoveSpecialCharacters(this string str)
    {
        var sb = new StringBuilder();
        foreach (char c in str)
        {
            if (((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) && (c != '.' && c != '_' && c != ' '))
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    public static bool ContainsIllegalCharacters(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return false;
        }

        foreach (var character in _escapeCharacters)
        {
            if (!str.Contains(character))
            {
                continue;
            }
            return true;
        }
        foreach (var character in _illegalCharacters)
        {
            if (!str.Contains(character))
            {
                continue;
            }
            return true;
        }
        return false;
    }
    
    public static bool ContainsIllegalFileCharacters(this string str)
    {
        if(string.IsNullOrWhiteSpace(str))
        {
            return false;
        }

        foreach (var character in _escapeCharacters)
        {
            if (!str.Contains(character))
            {
                continue;
            }
            return true;
        }
        foreach (var character in _illegalFileCharacters)
        {
            if (!str.Contains(character))
            {
                continue;
            }
            return true;
        }
        return false;
    }
}
