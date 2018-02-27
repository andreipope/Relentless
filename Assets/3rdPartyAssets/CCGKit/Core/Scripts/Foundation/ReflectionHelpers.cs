// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// Miscellaneous reflection utilities.
    /// </summary>
    public static class ReflectionHelpers
    {
        /// <summary>
        /// Returns all the types derived from the specified type.
        /// </summary>
        /// <param name="aAppDomain">The app domain.</param>
        /// <param name="aType">The base type.</param>
        /// <returns>All the types derived from the specified type.</returns>
        public static System.Type[] GetAllDerivedTypes(this System.AppDomain aAppDomain, System.Type aType)
        {
            var result = new List<System.Type>();
            var assemblies = aAppDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(aType))
                    {
                        result.Add(type);
                    }
                }
            }
            return result.ToArray();
        }
    }
}
