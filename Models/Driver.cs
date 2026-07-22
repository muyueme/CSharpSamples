using Models.Interface;

namespace Models
{
    public class Driver
    {
        private IDrive _vehicle;

        public Driver(IDrive vehicle)
        {
            this._vehicle = vehicle;
        }

        public void Drive()
        {
            this._vehicle.Run();
        }
    }
}
