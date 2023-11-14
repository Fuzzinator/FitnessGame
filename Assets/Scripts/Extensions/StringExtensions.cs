using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class StringExtensions
{
    private static string[] _illegalCharacters = new[] {":", "*", "?", "\"", "<", ">", "|"};
    
    public static string RemoveIllegalIOCharacters(this string folderName)
    {
        foreach (var character in _illegalCharacters)
        {
            if (!folderName.Contains(character))
            {
                continue;
            }

            folderName = folderName.Replace(character, string.Empty);
        }
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
}
