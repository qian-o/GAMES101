using Maths;
using Silk.NET.Assimp;
using AssimpMesh = Silk.NET.Assimp.Mesh;
using AssimpScene = Silk.NET.Assimp.Scene;

namespace PA7;

internal static unsafe class AssimpParsing
{
    public static Mesh[] Parsing(string file, Material material)
    {
        const PostProcessSteps optimizeSteps = PostProcessSteps.CalculateTangentSpace
                                               | PostProcessSteps.Triangulate
                                               | PostProcessSteps.GenerateNormals
                                               | PostProcessSteps.GenerateSmoothNormals
                                               | PostProcessSteps.GenerateUVCoords
                                               | PostProcessSteps.OptimizeMeshes
                                               | PostProcessSteps.OptimizeGraph
                                               | PostProcessSteps.PreTransformVertices;

        using Assimp importer = Assimp.GetApi();
        AssimpScene* scene = importer.ImportFile(file, (uint)optimizeSteps);

        if (scene == null)
        {
            throw new Exception("Failed to load model");
        }

        int numTriangles = 0;
        List<Mesh> meshes = [];

        ProcessNode(scene->MRootNode);

        Console.WriteLine($"Loaded model: {file}");
        Console.WriteLine($"Number of triangles: {numTriangles}");

        return [.. meshes];

        void ProcessNode(Node* node)
        {
            for (uint i = 0; i < node->MNumMeshes; i++)
            {
                AssimpMesh* mesh = scene->MMeshes[node->MMeshes[i]];

                meshes.Add(ProcessMesh(mesh));
            }

            for (uint i = 0; i < node->MNumChildren; i++)
            {
                ProcessNode(node->MChildren[i]);
            }
        }

        Mesh ProcessMesh(AssimpMesh* mesh)
        {
            Vector3d[] vectors = new Vector3d[mesh->MNumVertices];

            for (uint i = 0; i < mesh->MNumVertices; i++)
            {
                vectors[i] = (*&mesh->MVertices[i]).ToMaths();
            }

            uint[] indices = new uint[mesh->MNumFaces * 3];

            for (uint i = 0; i < mesh->MNumFaces; i++)
            {
                Face face = mesh->MFaces[i];

                for (uint j = 0; j < face.MNumIndices; j++)
                {
                    indices[(i * 3) + j] = face.MIndices[j];
                }
            }

            List<Triangle> triangles = [];

            for (int i = 0; i < indices.Length; i += 3)
            {
                Triangle triangle = new(vectors[indices[i]], vectors[indices[i + 1]], vectors[indices[i + 2]], material);

                triangles.Add(triangle);
            }

            numTriangles += triangles.Count;

            return new Mesh([.. triangles], material);
        }
    }
}