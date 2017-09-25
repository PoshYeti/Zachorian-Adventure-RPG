using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum TileType
{
	//when null
	Empty = -1,
	//calculated when neighbors on all sides
	Grass = 15,
	Trees = 16,
	Hills = 17,
	Mountains = 18,
	Towns = 19,
	Castle = 20,
	Dungeons = 21
}

public class Map
{
	public Tile[] tiles;
	public int columns;
	public int rows;

	public Tile[] coastTiles
	{
		get
		{ 
			return tiles.Where (t => t.autotileID < (int)TileType.Grass).ToArray ();
		}
	}

	public Tile[] landTiles
	{
		get
		{
			return tiles.Where (t => t.autotileID == (int)TileType.Grass).ToArray ();
		}
	}

	public Tile castleTile
	{
		get
		{
			return tiles.FirstOrDefault (t => t.autotileID == (int)TileType.Castle);
		}
	}

	public void NewMap(int width, int height)
	{
		columns = width;
		rows = height;

		tiles = new Tile[columns * rows];

		CreateTiles ();
	}

	//Erodes the edges of the map, to generate water
	public void CreateIsland(float erodePercent, int erodeIterations, float treePercent, float hillPercent, 
		float mountainPercent, float townPercent, float dungeonPercent, float lakePercent)
	{
		//Generates lakes on the island
		DecorateTiles (landTiles, lakePercent, TileType.Empty);

		//Erodes the edges to create a coastline
		for (var i = 0; i < erodeIterations; i++) 
		{
			DecorateTiles (coastTiles, erodePercent, TileType.Empty);
		}

		//Generate a caste at an open spot
		var openTiles = landTiles;
		RandomizeTileArray (openTiles);
		openTiles [0].autotileID = (int)TileType.Castle;

		//Priority 1
		DecorateTiles (landTiles, treePercent, TileType.Trees);
		//Priority 2
		DecorateTiles (landTiles, hillPercent, TileType.Hills);
		//Priority 3
		DecorateTiles (landTiles, mountainPercent, TileType.Mountains);
		//Priority 4
		DecorateTiles (landTiles, townPercent, TileType.Towns);
		//Priority 5
		DecorateTiles (landTiles, dungeonPercent, TileType.Dungeons);
	}

	private void CreateTiles()
	{
		var total = tiles.Length;

		for (var i = 0; i < total; i++) 
		{
			var tile = new Tile ();
			tile.id = i;
			tiles [i] = tile;
		}

		FindNeighbors ();
	}

	private void FindNeighbors()
	{
		for (var r = 0; r < rows; r++) 
		{
			for (var c = 0; c < columns; c++) 
			{
				var tile = tiles [columns * r + c];

				//Checks if there's a neighbor to the bottom of the current tile
				if (r < rows - 1) 
				{
					tile.AddNeighbor (Sides.Bottom, tiles [columns * (r + 1) + c]);
				}
				//Checks if there's a neighbor to the right of the current tile
				if (c < columns - 1) 
				{
					tile.AddNeighbor (Sides.Right, tiles [columns * r + (c + 1)]);
				}
				//Checks if there's a neighbor to the left of the current tile
				if (c > 0) 
				{
					tile.AddNeighbor (Sides.Left, tiles [columns * r + (c - 1)]);
				}
				//Checks if there's a neighbor to the top of the current tile
				if (r > 0) 
				{
					tile.AddNeighbor (Sides.Top, tiles [columns * (r - 1) + c]);
				}
			}
		}
	}

	public void DecorateTiles(Tile[] tiles, float percent, TileType type)
	{
		var total = Mathf.FloorToInt (tiles.Length * percent);

		RandomizeTileArray (tiles);

		for (var i = 0; i < total; i++) 
		{
			var tile = tiles [i];

			//turning a tile to an ocean tile
			if (type == TileType.Empty) 
			{
				tile.ClearNeighbors ();
			}

			tile.autotileID = (int)type;
		}
	}

	//Fisher-Yates shuffel argorithm
	public void RandomizeTileArray(Tile[] tiles)
	{
		//Going through array, swapping random elements
		for (var i = 0; i < tiles.Length; i++) 
		{
			var temp = tiles [i];
			var r = Random.Range (i, tiles.Length);
			tiles [i] = tiles [r];
			tiles [r] = temp;
		}
	}
}