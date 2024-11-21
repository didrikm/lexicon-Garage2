using lexicon_Garage2.Data;
using lexicon_Garage2.Models;
using lexicon_Garage2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace lexicon_Garage2.Controllers
{
    [Authorize]
    public class VehiclesController : Controller
    {
        private readonly lexicon_Garage2Context _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public readonly decimal ParkingHourlyPrice = 100;
        public static decimal AccumulatedParkingRevenue { get; set; }

        public VehiclesController(
            UserManager<ApplicationUser> userManager,
            lexicon_Garage2Context context
        )
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<ParkingSpotViewModel>> GetParkingStatusAsync()
        {
            List<Vehicle> vehicles = await _context
                .Vehicles.Where(v => v.ParkingSpots.Any())
                .ToListAsync();

            List<ParkingSpotViewModel> parkingStatus = new();

            List<ParkingSpot> parkingSpots = await _context.ParkingSpots.ToListAsync();

            foreach (var parkingSpot in parkingSpots)
            {
                var vehicleInSpot = vehicles.FirstOrDefault(v =>
                    v.ParkingSpots.Contains(parkingSpot)
                );
                {
                    parkingStatus.Add(
                        new ParkingSpotViewModel
                        {
                            SpotNumber = (parkingSpots.IndexOf(parkingSpot) + 1),
                            IsOccupied = vehicleInSpot != null,
                            RegistrationNumber =
                                vehicleInSpot?.RegistrationNumber // Assign registration number if occupied
                            ,
                        }
                    );
                }
            }

            return parkingStatus;
        }

        public async Task<int> GetAvailableSpotsAsync()
        {
            int occipiedSpots = 0;

            foreach (var vehicle in await _context.Vehicles.ToListAsync())
            {
                occipiedSpots += vehicle.Size;
            }

            var totalSpots = await _context.ParkingSpots.ToListAsync();

            return totalSpots.Count() - occipiedSpots;
        }

        private async Task<ParkingSpot?> GetNextAvailableSpotAsync()
        {
            var firstAvailableSpot = await _context.ParkingSpots.FirstOrDefaultAsync(ps =>
                ps.Vehicle == null
            );

            if (firstAvailableSpot == null)
            {
                return null; // Om alla platser är upptagna
            }
            return firstAvailableSpot; // Returnera första lediga plats(om det finns någon)
        }

        //Member view Point.6 Garage 3.0
        public IActionResult Members(string searchQuery, string sortOrder)
        {
            decimal parkingHourlyPrice = 5.00m; // Pris per timme
            var members = _context
                .Users.Include(u => u.Vehicles) // Hämtar användare och deras fordon
                .ToList()
                .Select(user => new MemberViewModel(user, parkingHourlyPrice)) // Mappar till MemberViewModel
                .ToList();

            // Sökfunktion
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                members = members
                    .Where(m => m.Owner.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Sorteringsfunktion
            members =
                sortOrder?.ToLower() == "desc"
                    ? members.OrderByDescending(m => m.Owner).ToList()
                    : members.OrderBy(m => m.Owner).ToList();

            return View(members);
        }

        public async Task<IActionResult> ParkedVehicle()
        {
            // Hämta alla fordon och inkludera relaterad användare
            var parkedVehicles = await _context
                .Vehicles.Include(v => v.ApplicationUser) // Inkludera navigeringsegenskapen för ägaren
                .ToListAsync();

            // Skapa en lista av ViewModels baserad på de hämtade fordonen
            var viewModel = parkedVehicles
                .Where(vehicle => vehicle.ApplicationUser != null) // Exkludera poster utan ägare
                .Select(vehicle => new ParkedVehicleViewModel(
                    vehicle,
                    vehicle.ApplicationUser // Skicka relaterad ApplicationUser som ägare
                ))
                .ToList();

            // Returnera vyn med ViewModel
            return View(viewModel);
        }

        public async Task<IActionResult> ParkingSpot()
        {
            ViewBag.AvailableSpots = await GetAvailableSpotsAsync();
            ViewBag.ParkingStatus = await GetParkingStatusAsync();
            var parkingStatus = await GetParkingStatusAsync();
            return View(parkingStatus); // Pass the list of ParkingSpotViewModel
        }

        // GET: Vehicles
        public async Task<IActionResult> Admin()
        {
            return View(await _context.Vehicles.ToListAsync());
        }

        // GET: Garage
        public async Task<IActionResult> Garage(
            string? searchTerm = null,
            string sortColumn = "ArrivalTime",
            string sortOrder = "asc",
            string? timeFilter = null
        )
        {
            ViewBag.AvailableSpots = await GetAvailableSpotsAsync();
            ViewBag.ParkingStatus = await GetParkingStatusAsync(); // Lägger till status för lediga/upptagna platser

            IQueryable<Vehicle> vehicles = _context.Vehicles;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (await _context.Vehicles.AnyAsync(v => v.VehicleType.TypeName == searchTerm))
                {
                    vehicles = vehicles.Where(v => v.VehicleType.TypeName == searchTerm);
                }
                else
                {
                    vehicles = vehicles.Where(v => v.RegistrationNumber.Contains(searchTerm));
                }
            }

            if (!string.IsNullOrEmpty(timeFilter))
            {
                var now = DateTime.Now;
                vehicles = timeFilter switch
                {
                    "minute" => vehicles.Where(v => v.ParkingTime >= now.AddMinutes(-1)),
                    "hour" => vehicles.Where(v => v.ParkingTime >= now.AddHours(-1)),
                    "day" => vehicles.Where(v => v.ParkingTime >= now.AddDays(-1)),
                    _ => vehicles,
                };
            }

            vehicles = sortColumn switch
            {
                "RegistrationNumber" => sortOrder == "asc"
                    ? vehicles.OrderBy(v => v.RegistrationNumber)
                    : vehicles.OrderByDescending(v => v.RegistrationNumber),
                "VehicleType" => sortOrder == "asc"
                    ? vehicles.OrderBy(v => v.VehicleType)
                    : vehicles.OrderByDescending(v => v.VehicleType),
                "ArrivalTime" => sortOrder == "asc"
                    ? vehicles.OrderBy(v => v.ParkingTime)
                    : vehicles.OrderByDescending(v => v.ParkingTime),
                "ParkingSpot" => sortOrder == "asc"
                    ? vehicles.OrderBy(v => v.ParkingSpots)
                    : vehicles.OrderByDescending(v => v.ParkingSpots),
                _ => vehicles.OrderBy(v => v.ParkingTime),
            };

            var vehicleViewModels = await vehicles
                .Select(vehicle => new VehicleViewModel(vehicle))
                .ToListAsync();

            ViewData["CurrentSort"] = $"{sortColumn}_{sortOrder}";

            return View(vehicleViewModels);
        }

        public async Task<IActionResult> Filter(string registrationNumber)
        {
            var model = string.IsNullOrWhiteSpace(registrationNumber)
                ? _context.Vehicles
                : _context.Vehicles.Where(m => m.RegistrationNumber.Contains(registrationNumber));

            return View(nameof(Admin), await model.ToListAsync());
        }

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // GET: Vehicles/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("VehicleType,RegistrationNumber,Color,Brand,Model,NumberOfWheels")]
                VehicleViewModel vehicleViewModel
        )
        {
            // Check if there are available spots
            var nextAvailableSpot = await GetNextAvailableSpotAsync();
            if (nextAvailableSpot == null)
            {
                TempData["ErrorMessage"] = "The garage is full - no available spots.";
                return RedirectToAction(nameof(Garage));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the currently logged-in user
                    var currentUser = await _userManager.GetUserAsync(User);

                    if (currentUser == null)
                    {
                        TempData["ErrorMessage"] = "User not found.";
                        return RedirectToAction(nameof(Garage));
                    }

                    // Map the VehicleViewModel to the Vehicle entity
                    var vehicle = new Vehicle
                    {
                        VehicleType = vehicleViewModel.VehicleType,
                        RegistrationNumber = vehicleViewModel.RegistrationNumber,
                        Color = vehicleViewModel.Color,
                        Brand = vehicleViewModel.Brand,
                        Model = vehicleViewModel.Model,
                        NumberOfWheels = vehicleViewModel.NumberOfWheels,
                        ApplicationUserId = currentUser.Id, // Set the UserId (Foreign Key)
                        ApplicationUser = currentUser, // Set the navigation property
                        //ParkingSpot = nextAvailableSpot,
                        ParkingTime = DateTime.Now,
                    };

                    // Add the vehicle to the context and save changes
                    vehicle.ParkingSpots.Add(nextAvailableSpot); // Tilldela den lediga platsen till fordonet
                    _context.Add(vehicle);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Parking has started.";
                    return RedirectToAction(nameof(Garage));
                }
                catch (DbUpdateException ex)
                    when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
                {
                    ModelState.AddModelError(
                        "RegistrationNumber",
                        "A vehicle with this registration number is already parked."
                    );
                }
                catch (Exception ex)
                {
                    // Log or handle any other exception
                    Console.WriteLine("Error: ", ex);
                    TempData["ErrorMessage"] = "An unexpected error occurred.";
                }
            }

            TempData["ErrorMessage"] = "Could not park the vehicle. Please check your inputs.";
            return View(vehicleViewModel);
        }

        // GET: Vehicles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Vehicles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,VehicleType,RegistrationNumber,Color,Brand,Model,NumberOfWheels,ParkingTime")]
                Vehicle vehicle
        )
        {
            if (id != vehicle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the original vehicle data, including the parking spot
                    var existingVehicle = await _context
                        .Vehicles.AsNoTracking()
                        .FirstOrDefaultAsync(v => v.Id == id);

                    if (existingVehicle == null)
                    {
                        return NotFound();
                    }

                    // Preserve original ParkingTime and ParkingSpot
                    vehicle.ParkingTime = existingVehicle.ParkingTime;
                    vehicle.ParkingSpots = existingVehicle.ParkingSpots;

                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "The vehicle has been updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await VehicleExists(vehicle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Garage));
            }

            TempData["ErrorMessage"] = "Could not update. Please check your inputs.";
            return View(vehicle);
        }

        // GET: Vehicles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context
                .Vehicles.Include(v => v.ParkingSpots)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle != null)
            {
                // Create the receipt view model before the vehicle gets changed
                var receiptViewModel = new ReceiptViewModel(vehicle, ParkingHourlyPrice);

                // Remove the vehicle
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Parking has ended.";

                AccumulatedParkingRevenue += receiptViewModel.Total;
                return View("Receipt", receiptViewModel);
            }
            else
            {
                TempData["ErrorMessage"] = "Could not find the vehicle.";
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Garage));
        }

        // GET: Statistics
        public async Task<IActionResult> Statistics()
        {
            var vehicles = await _context.Vehicles.ToListAsync();
            var vehicleStats = new StatisticsViewModel
            {
                NumberOfVehiclesParked = vehicles.Count,
                NumberOfWheelsInGarage = vehicles.Sum(v => v.NumberOfWheels),
                UnrealizedParkingRevenue =
                    vehicles.Sum(v => (decimal)(DateTime.Now - v.ParkingTime).TotalHours)
                    * ParkingHourlyPrice,
                AccumulatedParkingRevenueView = AccumulatedParkingRevenue,
            };

            return View(vehicleStats);
        }

        private async Task<bool> VehicleExists(int id)
        {
            return await _context.Vehicles.AnyAsync(v => v.Id == id);
        }
    }
}
