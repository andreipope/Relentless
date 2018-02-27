// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A collection of miscellaneous string-related utilities.
/// </summary>
public static class StringUtils
{
    public static string DisplayCamelCaseString(string camelCase)
    {
        var chars = new List<char>();
        chars.Add(camelCase[0]);
        foreach (var c in camelCase.Skip(1))
        {
            if (char.IsUpper(c))
            {
                chars.Add(' ');
                chars.Add(char.ToLower(c));
            }
            else
            {
                chars.Add(c);
            }
        }

        return new string(chars.ToArray());
    }
}
