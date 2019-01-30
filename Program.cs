using System;

namespace TQ.Mesh_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var nothing = Span<byte>.Empty;
            foreach (var part in new Mesh.Mesh(nothing))
            {

            }
        }
    }
}
