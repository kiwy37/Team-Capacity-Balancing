using System;

namespace TeamCapacityBalancing.Navigation;

[AttributeUsage(AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
public sealed class InjectAttribute : Attribute
{
}
