namespace GremlinDemo
{
    internal class Client : Location
    {
        public string Name { get; set; }

        public Client(string name, int x, int y) : base(x, y)
        {
            Name = name;
        }
    }
}
