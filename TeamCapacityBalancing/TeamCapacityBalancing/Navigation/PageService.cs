using System;
using System.Collections.Generic;

namespace TeamCapacityBalancing.Navigation;

public class PageService
{
    public Dictionary<Type, PageData> Pages { get; } = new();

    public void RegisterPage<P, VM>(string pageName)
    {
        Pages.Add(typeof(P), new PageData()
        {
            Name = pageName,
            ViewModelType = typeof(VM),
            Type = typeof(P),
        });

    }

}
