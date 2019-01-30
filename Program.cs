using System;
using System.IO;
using TQ.Mesh.Parts;
using static TQ.Mesh.Parts.VertexBuffer;

namespace TQ.Mesh_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Span<byte> file = File.ReadAllBytes(args.Length > 0 ? args[0] : "../../../mesh.msh");
            foreach (var part in new Mesh.Mesh(file))
            {
                if (part.Is(out MIF mif))
                {
                    Console.WriteLine("=== MIF ===");
                    Console.WriteLine(mif.Text);
                }
                else if (part.Is(out TextData textData))
                {
                    Console.WriteLine("=== Text Data ===");
                    Console.WriteLine(textData.Text);
                }
                else if (part.Is(out VertexBuffer vertexBuffer))
                {
                    Console.WriteLine("=== Vertex Buffer ===");
                    Console.WriteLine($"{nameof(vertexBuffer.Chunks)}:");
                    foreach (var chunk in vertexBuffer.Chunks)
                        Console.WriteLine($"  {Enum.GetName(typeof(ChunkId), chunk)} ({GetChunkSize(chunk)} bytes)");
                    Console.WriteLine($"{nameof(vertexBuffer.Buffer)}: [ {vertexBuffer.Buffer.Length} bytes ]");
                }
                else if (part.Is(out Extents extents))
                {
                    Console.WriteLine("=== Extents ===");
                    Console.WriteLine($"X: {extents.MinX}, {extents.MaxX}");
                    Console.WriteLine($"Y: {extents.MinY}, {extents.MaxY}");
                    Console.WriteLine($"Z: {extents.MinZ}, {extents.MaxZ}");
                }
                else if (part.Is(out Span<Bone> bones))
                {
                    Console.WriteLine("=== Bones ===");
                    Console.WriteLine($"Count: {bones.Length}");
                    foreach (ref var bone in bones)
                    {
                        Console.WriteLine("--- Bone ---");
                        Console.WriteLine($"Name: {bone.Name}");
                        Console.WriteLine($"FirstChild: {bone.FirstChild}");
                        Console.WriteLine($"ChildCount: {bone.ChildCount}");
                        unsafe
                        {
                            Console.WriteLine("Axes:");
                            fixed (float* axes = bone.Axes)
                            {
                                for (int i = 0; i < 3; i++)
                                    Console.WriteLine($" [{axes[i * 3]}, {axes[i * 3 + 1]}, {axes[i * 3 + 2]}]");
                            }
                            Console.WriteLine("Position:");
                            fixed (float* position = bone.Position)
                            { Console.WriteLine($" [{position[0]}, {position[1]}, {position[2]}]"); }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"=== {part.Id} ===");
                    Console.WriteLine($"[ {part.Data.Length} bytes ]");
                }
            }
        }
    }
}
