using System.Collections.Generic;
using UnityEngine;

namespace NRatel.TextureUnpacker
{
    public struct SizeInt
    {
        public int width;
        public int height;
        public SizeInt(Vector2Int v2i)
        {
            this.width = v2i.x;
            this.height = v2i.y;
        }
    }

    public struct Metadata
    {
        public int format;
        public SizeInt size;
        public string textureFileName;

        public Metadata(int format, SizeInt size, string textureFileName)
        {
            this.format = format;
            this.size = size;
            this.textureFileName = textureFileName;
        }
    }

    public struct Frame
    {
        public string textureName;
        public Vector2Int startPos;
        public SizeInt size;
        public SizeInt sourceSize;
        public bool isRotated;
        public Vector2Int offset;

        public Frame(string textureName, Vector2Int startPos, SizeInt size, SizeInt sourceSize, bool isRotated, Vector2Int offset)
        {
            this.textureName = textureName;
            this.startPos = startPos;
            this.size = size;
            this.sourceSize = sourceSize;
            this.isRotated = isRotated;
            this.offset = offset;
        }
    }

    public struct Plist
    {
        public string version;
        public string path;
        public Metadata metadata;
        public List<Frame> frames;

        public Plist(string version, string path, Metadata metadata, List<Frame> frames)
        {
            this.version = version;
            this.path = path;
            this.metadata = metadata;
            this.frames = frames;
        }
    }
}

