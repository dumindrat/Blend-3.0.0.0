using System.Collections.Generic;

namespace Blend.DataService.Mapping
{
    public interface IStoredProcedureMapper
    {
        T? QuerySingleOrDefault<T>(string procedureName, object? parameters = null) where T : class;
        IReadOnlyList<T> Query<T>(string procedureName, object? parameters = null);
        int Execute(string procedureName, object? parameters = null);
    }
}
