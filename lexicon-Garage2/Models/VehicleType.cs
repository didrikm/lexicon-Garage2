namespace lexicon_Garage2.Models
{
    public class VehicleType
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public int Size { get; set; }
        public ICollection<Vehicle> Vehicles { get; set; }
    }
}
