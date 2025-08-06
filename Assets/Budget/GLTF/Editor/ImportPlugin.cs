using System;
using System.Collections.Generic;
using GLTF.Schema;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;
using UnityGLTF;
using UnityGLTF.Plugins;

namespace Budget.GLTF
{
    public class ImportPluginContext : GLTFImportPluginContext
    {
        private static string RelativePathFrom(Transform self, Transform root)
        {
            var path = new List<string>();
            for (var current = self; current != null; current = current.parent)
            {
                if (current == root)
                {
                    return string.Join("/", path.ToArray());
                }

                path.Insert(0, current.name);
            }

            throw new Exception("no RelativePath");
        }

        private static AnimationClip CreateAnimationClip(GLTFSceneImporter importer, int animationIndex, GameObject scene)
        {
            var gltfAnimation = importer.Root.Animations[animationIndex];
            var gltfChanndels = gltfAnimation.Channels;

            var builder = new BlobBuilder(Allocator.Temp);
            ref Clip clip = ref builder.ConstructRoot<Clip>();
            BlobBuilderArray<Channel> channels = builder.Allocate(ref clip.Channels, gltfChanndels.Count);

            var nodes = new string[gltfChanndels.Count];
            for (int i = 0; i < gltfChanndels.Count; i++)
            {
                var gltfChanndel = gltfChanndels[i];
                var node = importer.NodeCache[gltfChanndel.Target.Node.Id];
                nodes[i] = RelativePathFrom(node.transform, scene.transform);

                var input_accessor = gltfChanndel.Sampler.Value.Input.Value;
                var input_length = input_accessor.Count;
                BlobBuilderArray<float> input = builder.Allocate(ref channels[i].Input, (int)input_length);
                {
                    var bufferData = importer.AnimationCache[animationIndex].Samplers[gltfChanndel.Sampler.Id].Input.bufferData;
                    unsafe
                    {
                        var input_ptr = (byte*)bufferData.GetUnsafePtr() + input_accessor.ByteOffset + input_accessor.BufferView.Value.ByteOffset;
                        UnsafeUtility.MemCpy(input.GetUnsafePtr(), input_ptr, input_length * 4);
                    }
                }

                var output_accessor = gltfChanndel.Sampler.Value.Output.Value;
                uint output_length;
                switch (output_accessor.Type)
                {
                    case GLTFAccessorAttributeType.VEC3:
                        output_length = output_accessor.Count * 3;
                        break;
                    case GLTFAccessorAttributeType.VEC4:
                        output_length = output_accessor.Count * 4;
                        break;
                    default:
                        throw new Exception($"unsupported output type: {output_accessor.Type}");
                }
                BlobBuilderArray<float> output = builder.Allocate(ref channels[i].Output, (int)output_length);
                {
                    var bufferData = importer.AnimationCache[animationIndex].Samplers[gltfChanndel.Sampler.Id].Output.bufferData;
                    unsafe
                    {
                        var output_ptr = (byte*)bufferData.GetUnsafePtr() + output_accessor.ByteOffset + output_accessor.BufferView.Value.ByteOffset;
                        UnsafeUtility.MemCpy(output.GetUnsafePtr(), output_ptr, output_length * 4);
                    }
                }

                switch (gltfChanndel.Target.Path)
                {
                    case "translation":
                        channels[i].Path = ChannelPath.TRANSLATION;
                        break;
                    case "rotation":
                        channels[i].Path = ChannelPath.ROTATION;
                        break;
                    case "scale":
                        channels[i].Path = ChannelPath.SCALE;
                        break;
                    case "weights":
                    default:
                        throw new Exception($"unsupported channel path: {gltfChanndel.Target.Path}");
                }
            }

            var animationClip = ScriptableObject.CreateInstance<AnimationClip>();
            animationClip.name = gltfAnimation.Name;
            animationClip.Nodes = nodes;
            animationClip.Blob = builder.CreateBlobAssetReference<Clip>(Allocator.Persistent);
            builder.Dispose();

            return animationClip;
        }

        private GLTFImportContext _context;

        public ImportPluginContext(GLTFImportContext context)
        {
            _context = context;
        }

        public override void OnBeforeImport()
        {
            _context.SceneImporter.CustomShaderName = "Budget/Phong";
        }

        public override void OnAfterImportNode(Node node, int nodeIndex, GameObject nodeObject)
        {
            var renderer = nodeObject.GetComponent<Renderer>();
            if (renderer)
            {
                if (renderer is UnityEngine.MeshRenderer)
                {
                    nodeObject.AddComponent<MeshRendererAuthoring>();
                }
            }
        }

        public override void OnAfterImportMaterial(GLTFMaterial material, int materialIndex, Material materialObject)
        {
            var pbr = material.PbrMetallicRoughness;
            if (pbr != null)
            {
                var baseColor = pbr.BaseColorFactor;
                if (baseColor != null)
                {
                    materialObject.SetColor("_BaseColor", new(baseColor.R, baseColor.G, baseColor.B, baseColor.A));
                }
                var baseColorTexture = pbr.BaseColorTexture;
                if (baseColorTexture != null)
                {
                    materialObject.SetTexture("_BaseMap", _context.SceneImporter.TextureCache[baseColorTexture.Index.Id].Texture);
                    materialObject.SetFloat("_BASEMAP", 1);
                }

                float smoothness = 1.0f - (float)pbr.RoughnessFactor;
                materialObject.SetFloat("_Smoothness", smoothness);

                // ignore Metallic
            }
        }

        public override void OnAfterImportScene(GLTFScene scene, int sceneIndex, GameObject sceneObject)
        {
            if (_context.Root.Animations != null)
            {
                for (int i = 0; i < _context.Root.Animations.Count; i++)
                {
                    var animationClip = CreateAnimationClip(_context.SceneImporter, i, sceneObject);
                    _context.AssetContext.AddObjectToAsset($"Budget_{animationClip.name}", animationClip);
                }

            }
            if (_context.Root.Skins != null)
            {
                var schemaSkin = _context.Root.Skins[0];

                var skinnedRenderer = sceneObject.GetComponentInChildren<UnityEngine.SkinnedMeshRenderer>();

                var skin = ScriptableObject.CreateInstance<Skin>();
                skin.name = schemaSkin.Name ?? "Skin_0";

                var joints = new string[skinnedRenderer.bones.Length];
                for (int i = 0; i < joints.Length; i++)
                {
                    joints[i] = RelativePathFrom(skinnedRenderer.bones[i].transform, sceneObject.transform);
                }
                skin.Joints = joints;
                {
                    var accessor = schemaSkin.InverseBindMatrices.Value;

                    var builder = new BlobBuilder(Allocator.Temp);
                    ref var inverseBindMatrices = ref builder.ConstructRoot<InverseBindMatrices>();
                    var data = builder.Allocate(ref inverseBindMatrices.Data, (int)accessor.Count);
                    var bindposes = skinnedRenderer.sharedMesh.bindposes;
                    for (int i = 0; i < bindposes.Length; i++)
                    {
                        data[i] = bindposes[i];
                    }
                    // Is there a cleaner way to access bufferData?
                    // var bufferData = _context.SceneImporter.AnimationCache[0].Samplers[0].Input.bufferData;
                    // unsafe
                    // {
                    //     var source = (byte*)bufferData.GetUnsafePtr() + accessor.ByteOffset + accessor.BufferView.Value.ByteOffset;
                    //     UnsafeUtility.MemCpy(data.GetUnsafePtr(), source, 64 * accessor.Count);
                    // }
                    skin.InverseBindMatrices = builder.CreateBlobAssetReference<InverseBindMatrices>(Allocator.Persistent);
                    builder.Dispose();
                }

                var skinAuthoring = sceneObject.AddComponent<SkinAuthoring>();
                skinAuthoring.Proto = skin;

                var materials = new Dictionary<Material, Material>();
                var skinnedRenderers = sceneObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var renderer in skinnedRenderers)
                {
                    if (!materials.TryGetValue(renderer.sharedMaterial, out Material material))
                    {
                        material = new Material(renderer.sharedMaterial)
                        {
                            shader = Shader.Find("Budget/Phong"),
                            enableInstancing = true
                        };
                        material.SetFloat("_SKINNING", 1);
                        materials.Add(renderer.sharedMaterial, material);

                        _context.AssetContext.AddObjectToAsset($"Budget_{material.name}", material);
                    }
                    var authoring = renderer.gameObject.AddComponent<SkinnedMeshRendererAuthoring>();
                    authoring.Material = material;
                    authoring.Skin = skinAuthoring;
                }

                _context.AssetContext.AddObjectToAsset($"Budget_{skin.name}", skin);
            }
        }
    }

    public class ImportPlugin : GLTFImportPlugin
    {
        public override string DisplayName => "Budget.GLTF.ImportPlugin";

        public override GLTFImportPluginContext CreateInstance(GLTFImportContext context)
        {
            return new ImportPluginContext(context);
        }
    }
}

