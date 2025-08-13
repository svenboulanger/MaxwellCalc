namespace MaxwellCalc.Workspaces;

/// <summary>
/// A key that can be used to describe user functions.
/// </summary>
/// <param name="Name">The name.</param>
/// <param name="Arguments">The number of arguments.</param>
public record struct UserFunctionKey(string Name, int Arguments)
{

}
