using System;
using System.IO;
using TQ.Mesh.Parts;
using static TQ.Mesh.Parts.Bones;
using static TQ.Mesh.Parts.VertexBuffer;
using System.Linq;
using System.Numerics;

namespace TQ.Mesh_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Span<byte> file = File.ReadAllBytes(args.Length > 0 ? args[0] : "mesh.msh");
            var mesh = new Mesh.Mesh(file);
            Console.WriteLine($"File Format Version: {mesh.Version}");
            foreach (var part in mesh)
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
                    Console.WriteLine($"{nameof(vertexBuffer.Attributes)}:");
                    foreach (var attribute in vertexBuffer.Attributes)
                        Console.WriteLine($"  {Enum.GetName(typeof(AttributeId), attribute)} ({GetAttributeSize(attribute)} bytes)");
                    Console.WriteLine($"{nameof(vertexBuffer.Buffer)}: [ {vertexBuffer.Buffer.Length} bytes ]");
                }
                else if (part.Is(out Span<Extents> extentsSpan))
                {
                    ref var extents = ref extentsSpan[0];
                    Console.WriteLine("=== Extents ===");
                    Console.WriteLine($"Min: {extents.Min}");
                    Console.WriteLine($"Max: {extents.Max}");
                }
                else if (part.Is(out Bones bones))
                {
                    Console.WriteLine("=== Bones ===");
                    Console.WriteLine($"Count: {bones.Count}");
                    PrintBoneTree(bones[0]);
                }
                else if (part.Is(out Unknown13 unknown13))
                {
                    Console.WriteLine("=== Unknown 13 ===");
                    foreach (var @byte in unknown13.Data)
                        Console.Write($"{@byte} ");
                    Console.WriteLine("\u0008");
                }
                else if (part.Is(out IndexBuffer indexBuffer))
                {
                    Console.WriteLine("=== Index Buffer ===");
                    Console.WriteLine($"Triangle Count: {indexBuffer.TriangleCount}");
                    Console.WriteLine($"Draw Call Count: {indexBuffer.DrawCallCount}");
                    Console.WriteLine($"Triangle Indices: [ {indexBuffer.TriangleIndices.Length} triangles ]");
                    foreach (var drawCall in indexBuffer)
                    {
                        Console.WriteLine("--- Draw Call ---");
                        Console.WriteLine($"Shader ID: {drawCall.Common.ShaderID}");
                        Console.WriteLine($"Start Face Index: {drawCall.Common.StartFaceIndex}");
                        Console.WriteLine($"Face Count: {drawCall.Common.FaceCount}");
                        if (mesh.Version == 11)
                        {
                            Console.WriteLine($"Sub Shader ID: {drawCall.At11.SubShader}");
                            Console.WriteLine($"Min: {drawCall.At11.Min}");
                            Console.WriteLine($"Max: {drawCall.At11.Max}");
                        }
                        Console.WriteLine(@$"Bones: {
                            drawCall
                            .BoneMap
                            .ToArray()
                            .Select(x => x.ToString())
                            .Aggregate((a, b) => a + ", " + b)
                        }");
                    }
                }
                else if (part.Is(out Span<Hitbox> hitboxes))
                {
                    Console.WriteLine("=== Hitboxes ===");
                    foreach (ref var hitbox in hitboxes)
                    {
                        Console.WriteLine("--- Hitbox ---");
                        Console.WriteLine($"Name: {hitbox.Name}");
                        Console.WriteLine($"Position 1: {hitbox.Position1}");
                        Console.WriteLine($"Axes: {hitbox.Axes.ToArray().Select(x => x.ToString()).Aggregate((a, b) => a + ", " + b)}");
                        Console.WriteLine($"Position 2: {hitbox.Position2}");
                        Console.WriteLine($"Unknown: {hitbox.Unknown.ToArray().Select(x => x.ToString()).Aggregate((a, b) => a + ", " + b)}");
                    }
                }
                else if (part.Is(out Shaders shaders))
                {
                    Console.WriteLine("=== Shaders ===");
                    foreach (var shader in shaders)
                    {
                        Console.WriteLine("--- Shader ---");
                        Console.WriteLine($"File Name: {shader.FileName}");
                        Console.WriteLine("Parameters:");
                        foreach (var parameter in shader)
                        {
                            Console.Write($"  {parameter.Name}: ");
                            if (parameter.ValueIs(out string @string)) Console.WriteLine($"\"{@string}\"");
                            else if (parameter.ValueIs(out float @float)) Console.WriteLine(@float);
                            else if (parameter.ValueIs(out Vector2 vector2)) Console.WriteLine(vector2);
                            else if (parameter.ValueIs(out Vector3 vector3)) Console.WriteLine(vector3);
                            else Console.WriteLine("[ Unknown Format ]");
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

        static void PrintBoneTree(Bone bone, string indentation = "")
        {
            Console.WriteLine($"{indentation}--- Bone ---");
            Console.WriteLine($"{indentation}Name: {bone.Name}");
            Console.WriteLine($"{indentation}ChildCount: {bone.ChildCount}");
            Console.WriteLine($"{indentation}Axes:");
            var axes = bone.Axes;
            for (int i = 0; i < 3; i++)
                Console.WriteLine($"{indentation}  [{axes[i * 3]}, {axes[i * 3 + 1]}, {axes[i * 3 + 2]}]");
            Console.WriteLine($"{indentation}Position:");
            var position = bone.Position;
            Console.WriteLine($"{indentation}  {position}");
            foreach (var childBone in bone)
                PrintBoneTree(childBone, indentation + "|");
        }
    }
}
