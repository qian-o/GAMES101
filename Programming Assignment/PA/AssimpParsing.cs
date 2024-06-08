using System.Numerics;
using Maths;
using PA.Graphics;
using Silk.NET.Assimp;

namespace PA;

public unsafe class AssimpParsing
{
    private readonly Dictionary<string, (Vertex[] Vertices, uint[] Indices)> _meshes;

    public AssimpParsing(string filePath)
    {
        const PostProcessSteps steps = PostProcessSteps.CalculateTangentSpace
                                       | PostProcessSteps.Triangulate
                                       | PostProcessSteps.GenerateNormals
                                       | PostProcessSteps.GenerateSmoothNormals
                                       | PostProcessSteps.GenerateUVCoords
                                       | PostProcessSteps.OptimizeMeshes
                                       | PostProcessSteps.OptimizeGraph
                                       | PostProcessSteps.PreTransformVertices;

        using Assimp importer = Assimp.GetApi();
        Scene* scene = importer.ImportFile(filePath, (uint)steps);

        if (scene == null)
        {
            throw new Exception("Failed to load model");
        }

        List<(string Name, Vertex[] Vertices, uint[] Indices)> meshes = [];

        ProcessNode(scene->MRootNode);

        _meshes = meshes.ToDictionary(x => x.Name, x => (x.Vertices, x.Indices));

        void ProcessNode(Node* node)
        {
            for (uint i = 0; i < node->MNumMeshes; i++)
            {
                Mesh* mesh = scene->MMeshes[node->MMeshes[i]];

                meshes.Add(ProcessMesh(mesh));
            }

            for (uint i = 0; i < node->MNumChildren; i++)
            {
                ProcessNode(node->MChildren[i]);
            }
        }

        (string Name, Vertex[] Vertices, uint[] Indices) ProcessMesh(Mesh* mesh)
        {
            Vertex[] vertices = new Vertex[mesh->MNumVertices];

            for (uint i = 0; i < mesh->MNumVertices; i++)
            {
                vertices[i].Position = (*&mesh->MVertices[i]).ToMaths();
                vertices[i].Normal = (*&mesh->MNormals[i]).ToMaths();

                if (mesh->MColors[0] != null)
                {
                    vertices[i].Color = (*&mesh->MColors[0][i]).ToMaths();
                }

                if (mesh->MTextureCoords[0] != null)
                {
                    Vector3 texCoord = (*&mesh->MTextureCoords[0][i]);

                    vertices[i].TexCoord = new Vector2d(texCoord.X, texCoord.Y);
                }
            }

            uint[] indices = new uint[mesh->MNumFaces * 3];

            for (uint i = 0; i < mesh->MNumFaces; i++)
            {
                Face face = mesh->MFaces[i];

                for (uint j = 0; j < face.MNumIndices; j++)
                {
                    indices[i * 3 + j] = face.MIndices[j];
                }
            }

            return (mesh->MName.AsString, vertices, indices);
        }
    }

    public string[] MeshNames => [.. _meshes.Keys];

    public Vertex[] Vertices(string name) => _meshes[name].Vertices;

    public uint[] Indices(string name) => _meshes[name].Indices;
}
