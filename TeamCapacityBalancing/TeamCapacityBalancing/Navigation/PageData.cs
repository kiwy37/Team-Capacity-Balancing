using System;

namespace TeamCapacityBalancing.Navigation;

public class PageData
{
    public required Type Type { get; init; }
    public required string Name { get; init; }
    public required Type ViewModelType { get; init; }

}
