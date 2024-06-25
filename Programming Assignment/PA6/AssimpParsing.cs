using System.Numerics;
using Maths;
using PA.Graphics;
using Silk.NET.Assimp;
using AssimpMesh = Silk.NET.Assimp.Mesh;
using AssimpScene = Silk.NET.Assimp.Scene;

namespace PA6;

internal static unsafe class AssimpParsing
{
    public static Model Parsing(string file, Matrix4x4d? model = null)
    {
        model ??= Matrix4x4d.Identity;

        const PostProcessSteps steps = PostProcessSteps.CalculateTangentSpace
                                       | PostProcessSteps.Triangulate
                                       | PostProcessSteps.GenerateNormals
                                       | PostProcessSteps.GenerateSmoothNormals
                                       | PostProcessSteps.GenerateUVCoords
                                       | PostProcessSteps.OptimizeMeshes
                                       | PostProcessSteps.OptimizeGraph
                                       | PostProcessSteps.PreTransformVertices;

        using Assimp importer = Assimp.GetApi();
        AssimpScene* scene = importer.ImportFile(file, (uint)steps);

        if (scene == null)
        {
            throw new Exception("Failed to load model");
        }

        List<Mesh> meshes = [];

        ProcessNode(scene->MRootNode);

        return new Model([.. meshes]);

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
            Vertex[] vertices = new Vertex[mesh->MNumVertices];

            for (uint i = 0; i < mesh->MNumVertices; i++)
            {
                vertices[i].Position = model.Value * (*&mesh->MVertices[i]).ToMaths();
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

            List<Triangle> triangles = [];

            for (int i = 0; i < indices.Length; i += 3)
            {
                triangles.Add(new(vertices[indices[i]], vertices[indices[i + 1]], vertices[indices[i + 2]]));
            }

            return new Mesh([.. triangles]);
        }
    }
}
