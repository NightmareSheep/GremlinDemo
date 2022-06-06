namespace GremlinDemo
{
    internal class DistributionBox : Location
    {
        public int Capacity { get; set; }

        public DistributionBox(int capacity, int x, int y) : base(x, y)
        {
            Capacity = capacity;
        }
    }
}
