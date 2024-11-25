using Microsoft.AspNetCore.Mvc.Rendering;

namespace lexicon_Garage2.Services
{
    public interface IVehicleTypeService
    {
        Task<IEnumerable<SelectListItem>> GetVehicleTypesAsync();
    }
}