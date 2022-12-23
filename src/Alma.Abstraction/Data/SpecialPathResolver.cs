namespace Alma.Data;

public record SpecialPathResolver(string PathName, Func<string> Resolver, bool? SkipCombiningCurrentDirectory = null);