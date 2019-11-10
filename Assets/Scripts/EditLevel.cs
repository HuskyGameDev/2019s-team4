﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class EditLevel : MonoBehaviour {

	[ReadOnly]
	public bool drawing = false;
	// name and path to use when saving/loading level
	public string levelName = "test";
	public string levelPath = "/Levels";
	// safety check, must be checked to owerwrite a level that already exists
	public bool overwriteLevel;

	public Node.TileType type = Node.TileType.regular;	// type of tile that this should be changes to.
	public GameManager.Direction methodDirection;	// direction to create connection in / delete tile in
	public int linkX = 0;	// X & Y coordinates to link to in newly created chunk
	public int linkY = 0;	// Note the the coordinates start at (0,0) in the upper left corner
	public int linkToIndex = -1; // index of tile to link to, if not creating link to new chunk
	public enum LinkDirectionality { Twoway, Oneway };
	public LinkDirectionality linkDirectionality;   // indicates whether a created link should be oneway or twoway

	/*
	public byte newChunkColorR = 255;	// R, G, B, and Alpha components of color to give to newly created tiles
	public byte newChunkColorG = 255;
	public byte newChunkColorB = 255;
	private byte newChunkColorA = 255;*/
	public Color32 newChunkColor;
	public int newChunkWidth;	// how many tiles wide to make the new chunk
	public int newChunkHeight;	// how many tiles tall to make the new chunk
	
	public Node[,] chunkToLink;	// refernce to the most newly created chunk
	public Node[][,] chunks = new Node[0][,]; // probably not strictly needed since you cant access this to change things in the editor anyway

	public Node currentNode;    // referes to the copy of the current node
	public Map currentMap;  // refers to the current map use by the gameplay object

	private Color32 copyColorF;
	private Color32 copyColorW;
	private String copyfloorSprite;
	private String copywallSprite;
	private String copycornerSprite;

	/// <summary>
	/// Creates a new map, with a starting area created using the same settings as the chunk creation.
	/// (0,0) is the default source node.
	/// Note that if changes to the previous map haven't been saved, they will be lost.
	/// </summary>
	public void getNewMap() {
		Map tempMap = new Map();
		GameManager.instance.gameplay.map = tempMap;	// let gameplay & this have a new map
		currentMap = tempMap;
		createTileChunk(false);	// create a new tile chunk in the new map
		LevelEditor_2.setType(tempMap, 0, Node.TileType.source); // set the suorce tile to (0,0)
		//currentMap = tempMap;
		//GameManager.instance.gameplay.map = tempMap;
		GameManager.instance.gameplay.resetLevelAssets();	// reset player location & redraw everything
	}

	/// <summary>
	/// mostly called internally to make sure everything referes to the correct map
	/// </summary>
	public void getCurrentMap() {
		//Debug.Log("getCurrentMap() does not do anything right now");
		currentMap = GameManager.instance.gameplay.map;
	}

    /// <summary>
	/// Saves a level using the given name and filepath within the assest folder
	/// Note that this is a relative filepath from the assests folder, not the absolute filepath
	/// </summary>
	public void saveLevel() {
		//Debug.Log("WTF, is this not being called?");
		getCurrentMap();	// make sure map reference is current
		LevelEditor_2.cleanUpMap(currentMap);	// clean up the map, removing deleted or invalid tiles
		if (File.Exists(Application.dataPath + levelPath + "/room_" + levelName+".json")) {	// if it already exists, check wether the person intends to overwrite it
			if (overwriteLevel) {
				Map.Save(currentMap, Application.dataPath + levelPath + "/room_" + levelName+".json");
				Debug.Log("Overwriting level at: \"" + Application.dataPath + levelPath + "/room_" + levelName + "\" ?");
				overwriteLevel = false;
			} else {
				// If the overwriteLevel boolean is not set, do not overwrite the level
				Debug.Log("Are you sure you want to overwrite the level at: \"" + Application.dataPath + levelPath + "/room_" + levelName + "\" ?");
			}
		} else {
			// if it doesn't already exit, don't need to make any checks.
			Debug.Log("Saving level at: \"" + Application.dataPath + levelPath + "/room_" + levelName + "\"");
			Map.Save(currentMap, Application.dataPath + levelPath + "/room_" + levelName+".json");
		}

	}

	/// <summary>
	/// Loads a level using the given name and filepath within the assest folder.
	/// Can be used to load levels outside the Assets/Levels/ folder, unlike the ingame level loader
	/// </summary>
	public void loadLevelByName() {
		if (File.Exists(Application.dataPath + levelPath + "/room_" + levelName)) {
			currentMap = Map.Load(Application.dataPath + levelPath + "/room_" + levelName);
			GameManager.instance.gameplay.map = currentMap;
			GameManager.instance.gameplay.resetLevelAssets();
			Debug.Log("Loading level at: \"" + Application.dataPath + levelPath + "/room_" + levelName + "\"");
		} else {
			Debug.Log("Error: Map file does not exist at path \"" + Application.dataPath + levelPath + "/room_" + levelName + "\"");
		}
	}

	/// <summary>
	/// mostly used internaly to make sure that the copy of the current node is actually of the current node
	/// </summary>
	public void getCurrentNode() {
		currentNode = GameManager.instance.gameplay.currentPosition.Copy();
		//currentNode = GameManager.instance.gameplay.currentPosition;
	}

	/// <summary>
	/// apply any changes that have been made to the copy-of-the-current-node to the current node
	/// </summary>
	public void applyToNode() {
		getCurrentMap();    // make sure map reference is current
		currentMap[GameManager.instance.gameplay.currentPosition.index] = currentNode.Copy();
		GameManager.instance.gameplay.currentPosition = currentMap[GameManager.instance.gameplay.currentPosition.index];
		// draw changes to node
		GameManager.instance.gameplay.nonEuclidRenderer.HandleRender(GameManager.Direction.East, GameManager.instance.gameplay.currentPosition, false);
	}

	/// <summary>
	/// redraws the map, making changes visible
	/// </summary>
	public void redraw() {
		getCurrentMap();    // make sure map reference is current
		getCurrentNode();
		GameManager.instance.gameplay.nonEuclidRenderer.HandleRender(GameManager.Direction.East, GameManager.instance.gameplay.currentPosition, false);
	}


	/// <summary>
	/// creates a rectalngular matrix of tiles that are already linked together.
	/// Also creates a link to this new collection of tiles, from the current ile, and in 
	/// the indicated direction to the indicated (x,y) coordinates of the new chunk
	/// </summary>
	/// <param name="link"></param>
	public void createTileChunk(bool link = true) {
		getCurrentMap();    // make sure map reference is current
		Node[][,] temp = new Node[chunks.Length + 1][,];
		int i;
		for (i = 0; i < chunks.Length; i++) {
			temp[i] = chunks[i];
		}
		chunks = temp;
		// create new chunk
		//chunks[i] = LevelEditor_2.createChunk(currentMap, new Color32(newChunkColorR, newChunkColorG, newChunkColorB, newChunkColorA), newChunkWidth, newChunkHeight);
		chunks[i] = LevelEditor_2.createChunk(currentMap, newChunkColor, newChunkWidth, newChunkHeight);
		chunkToLink = chunks[i];
		if (link) {
			// create link to new chunk
			createLink(true);
		}
	}

	/// <summary>
	/// Creates a link from the current tile and in the direction given by methodDirection, to the
	/// node indicated by the linkToIndex. 
	/// Also called by the createTileChunk() method in order to ensure that the player can access the 
	/// newly created chunk, in which case it used the (x,y) coordinates given by (linkX, LinkY)
	/// </summary>
	/// <param name="newChunk"></param>
	public void createLink(bool newChunk = false) {
		getCurrentNode();   // make sure node reference is current
		getCurrentMap();    // make sure map reference is current

		if (newChunk) {	// if creating a link to a new chunk, use this method to create link
			if ((linkX >= 0) && (linkY >= 0) && (linkX < newChunkWidth) && (linkY < newChunkHeight)) {
				int toIndex = chunkToLink[(int)linkX, (int)linkY].index;
				LevelEditor_2.createTwoWayLink(currentMap, currentNode.index, toIndex, methodDirection);
			} else {
				Debug.Log("Error: The location to link to: (" + linkX + "," + linkY + ") the the new chunk the is " + newChunkWidth + "x" + newChunkHeight + " is not valid");
			}
		} else if ((linkToIndex >= 0) && (linkToIndex < currentMap.size) && (currentMap[linkToIndex] != null)) { // otherwisse use this method to create link
			if (linkDirectionality == LinkDirectionality.Twoway) {
				LevelEditor_2.createTwoWayLink(currentMap, currentNode.index, (int)linkToIndex, methodDirection);
			} else {
				LevelEditor_2.createOneWayLink(currentMap, currentNode.index, (int)linkToIndex, methodDirection);
			}
		} else {
			Debug.Log("Error: Given index of " + linkToIndex + " is not valid");
		}
		getCurrentNode(); // current node will have changed slightly, make sure copy is still accurate
		GameManager.instance.gameplay.nonEuclidRenderer.HandleRender(GameManager.Direction.East, currentNode, false);	// draw changes to map
	}

	/// <summary>
	/// 
	/// </summary>
	public void setType() {
		getCurrentNode();
		getCurrentMap();
		LevelEditor_2.setType(currentMap, currentNode.index, type);
		getCurrentNode();
		GameManager.instance.gameplay.nonEuclidRenderer.HandleRender(GameManager.Direction.East, currentNode, false);   // draw changes to map
	}

	/// <summary>
	/// Deletes the tile that is at the direction given by methodDirection.
	/// Can be used to screw up a map, so don't do that
	/// </summary>
	public void deleteTile() {
		//Debug.Log("deleteTile() does not do anything right now");
		getCurrentNode();	// make sure node copy is current
		getCurrentMap();	// make sure map reference is current
		int tempIndex = currentNode.GetConnectionFromDir(methodDirection);	// get the node that this one connects to in that direction
		if (tempIndex >= 0) {	// can only delete that node if it exists
			LevelEditor_2.deleteTile(currentMap, tempIndex);
			LevelEditor_2.cleanUpMap(currentMap);	// clean up the node array inside map, so that it doesn't have gaps in the array.
		} else {
			Debug.Log("Error: There is no node the " + methodDirection.ToString() + "-ern direction");
		}
		getCurrentNode();	// node copy now out of date, update it
		GameManager.instance.gameplay.nonEuclidRenderer.HandleRender(GameManager.Direction.East, currentNode, false);   // draw changes to map
	}

	public void sampleTile() {
		copyColorF = currentNode.colorF;
		copyColorW = currentNode.colorW;
		copyfloorSprite = currentNode.floorSprite;
		copywallSprite = currentNode.wallSprite;
		copycornerSprite = currentNode.cornerSprite;
	}
	public void setDrawStatus() {
		if (drawing) {
			drawing = false;
		} else {
			drawing = true;
		}
	}
	public void drawTiles() {
		if (drawing) {
			currentNode.colorF = copyColorF;
			currentNode.colorW = copyColorW;
			currentNode.floorSprite = copyfloorSprite;
			currentNode.wallSprite = copywallSprite;
			currentNode.cornerSprite = copycornerSprite;
			getCurrentMap();    // make sure map reference is current
			currentMap[GameManager.instance.gameplay.currentPosition.index] = currentNode.Copy();
			GameManager.instance.gameplay.currentPosition = currentMap[GameManager.instance.gameplay.currentPosition.index];
		}
	}
}
