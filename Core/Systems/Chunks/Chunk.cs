﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Core.Systems.Chunks
{
	public sealed class Chunk : IDisposable
	{
		public const int MaxChunkSize = 64;

		public readonly Point Position;
		public readonly long EncodedPosition;
		public readonly Rectangle TileRectangle;
		public readonly Rectangle WorldRectangle;

		private ChunkComponent[] components;

		internal Chunk(int x, int y)
		{
			//Positions

			Position = new Point(x, y);
			EncodedPosition = PackPosition(x, y);

			int xTilePos = Position.X * MaxChunkSize;
			int yTilePos = Position.Y * MaxChunkSize;

			TileRectangle = new Rectangle(
				xTilePos,
				yTilePos,
				Math.Min(Main.maxTilesX, xTilePos + MaxChunkSize) - xTilePos,
				Math.Min(Main.maxTilesY, yTilePos + MaxChunkSize) - yTilePos
			);

			WorldRectangle = new Rectangle(
				TileRectangle.X * TileUtils.TileSizeInPixels,
				TileRectangle.Y * TileUtils.TileSizeInPixels,
				TileRectangle.Width * TileUtils.TileSizeInPixels,
				TileRectangle.Height * TileUtils.TileSizeInPixels
			);

			//Components

			components = ChunkSystem.ChunkComponents
				.Select(c => c.Clone(this))
				.ToArray();
			
			for(int i = 0; i < components.Length; i++) {
				components[i].OnInit();
			}
		}

		public void Dispose()
		{
			if(components != null) {
				for(int i = 0; i < components.Length; i++) {
					components[i].Dispose();
				}

				components = null;
			}
		}
		public IEnumerable<ChunkComponent> EnumerateComponents()
		{
			for(int i = 0; i < components.Length; i++) {
				yield return components[i];
			}
		}
		public T GetComponent<T>() where T : ChunkComponent
		{
			return components[ModContent.GetInstance<T>().Id] as T ?? throw new Exception($"{nameof(ChunkComponent)} of type '{typeof(T).Name}' does not exist on the current chunk.");
		}

		public static long PackPosition(int x, int y) => (long)y << 32 | (uint)x;
	}
}
