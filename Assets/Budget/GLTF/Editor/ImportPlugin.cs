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
            BlobBuilderArray<Channel> channels = builder.Allocate(ref clip.channels, gltfChanndels.Count);

            var nodes = new string[gltfChanndels.Count];
            for (int i = 0; i < gltfChanndels.Count; i++)
            {
                var gltfChanndel = gltfChanndels[i];
                var node = importer.NodeCache[gltfChanndel.Target.Node.Id];
                nodes[i] = RelativePathFrom(node.transform, scene.transform);

                var input_accessor = gltfChanndel.Sampler.Value.Input.Value;
                var input_length = input_accessor.Count;
                BlobBuilderArray<float> input = builder.Allocate(ref channels[i].input, (int)input_length);
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
                BlobBuilderArray<float> output = builder.Allocate(ref channels[i].output, (int)output_length);
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
                        channels[i].path = ChannelPath.TRANSLATION;
                        break;
                    case "rotation":
                        channels[i].path = ChannelPath.ROTATION;
                        break;
                    case "scale":
                        channels[i].path = ChannelPath.SCALE;
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

        public override void OnAfterImportNode(Node node, int nodeIndex, GameObject nodeObject)
        {
            nodeObject.AddComponent<NameAuthoring>();

            var renderer = nodeObject.GetComponent<Renderer>();
            if (renderer)
            {
                if (renderer is UnityEngine.MeshRenderer)
                {
                    nodeObject.AddComponent<MeshRendererAuthoring>();
                }
                else if (renderer is UnityEngine.SkinnedMeshRenderer)
                {
                    nodeObject.AddComponent<SkinnedMeshRendererAuthoring>();
                }
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

