namespace Blend.BusinessService.TeaStandards;

public interface ITeaStandardService
{
    IReadOnlyList<TeaStandard> GetAllStandards(string searchText, int companyId);
    BusinessOperationResult CreateStandard(TeaStandard standard, int companyId);
    BusinessOperationResult UpdateStandard(TeaStandard standard);
    BusinessOperationResult DeleteStandard(int standardId);
}
