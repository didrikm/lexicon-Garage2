using lexicon_Garage2.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace lexicon_Garage2.Services
{
    public class VehicleTypeService : IVehicleTypeService
    {
        private readonly lexicon_Garage2Context context;

        public VehicleTypeService(lexicon_Garage2Context context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<SelectListItem>> GetVehicleTypesAsync()
        {
            var vehicleTypes = await context.VehicleTypes.ToListAsync();
            var selectList = vehicleTypes.Select(vt => new SelectListItem
            {
                Text = vt.TypeName + " " + vt.Size,
                Value = vt.Id.ToString(),
            });
            return selectList;
        }
    }
}
