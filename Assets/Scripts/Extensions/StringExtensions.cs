using System.Collections;
using System.Collections.Generic;
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
}
