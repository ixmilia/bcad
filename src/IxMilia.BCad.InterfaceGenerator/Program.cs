using IxMilia.BCad.Rpc;

namespace IxMilia.BCad.InterfaceGenerator
{
    class Program
    {
        public static void Main(string[] args)
        {
            var generator = new ContractGenerator(args);
            generator.Run();
        }
    }
}
