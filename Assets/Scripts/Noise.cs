using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {

	public static float[,] GenerateNoiseMap(int width, int height, NoiseData data, int seed) {
        float[,] noiseMap = new float[width, height];

        System.Random pseudoRandom = new System.Random (seed);
        Vector2[] octaveOffsets = new Vector2[data.octave];
        for(int i = 0; i < data.octave; i++) {
            float offsetX = pseudoRandom.Next (-100000, 100000) + data.offset.x;
            float offsetY = pseudoRandom.Next (-100000, 100000) + data.offset.y;
            octaveOffsets[i] = new Vector2 (offsetX, offsetY);
        }

        if (data.scale <= 0) {
            data.scale = 0.0001f;
        }

        float minNoiseHeight = float.MaxValue;
        float maxNoiseHeight = float.MinValue;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for(int i = 0; i < data.octave; i++) {
                    float sampleX = (x - halfWidth) / data.scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / data.scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= data.persistance;
                    frequency *= data.lacunarity;
                }

                if(noiseHeight > maxNoiseHeight) {
                    maxNoiseHeight = noiseHeight;
                } else if(noiseHeight < minNoiseHeight) {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                noiseMap[x, y] = Mathf.InverseLerp (minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}
