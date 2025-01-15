namespace ASCOM.Remote
{
    public class RateResponse(double minimum, double maximum)
    {
        private double maximum = maximum;
        private double minimum = minimum;

        public double Maximum
        {
            get { return this.maximum; }
            set { this.maximum = value; }
        }

        public double Minimum
        {
            get { return this.minimum; }
            set { this.minimum = value; }
        }
    }
}
