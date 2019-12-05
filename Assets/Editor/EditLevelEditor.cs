﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EditLevel), true)]
public class EditLevelEditor : Editor {
	private bool gotMap = false;
	private EditLevel EditLvl;// = (EditLevel) target;
	float thumbnailWidth = 70;
	float thumbnailHeight = 70;
	float labelWidth = 150f;

	public override void OnInspectorGUI() {
		// in the editor, get the reference to the map if you don't already have it
		if (Application.isPlaying && !gotMap) {
			((EditLevel)target).getCurrentMap();
			gotMap = true;
		}
		EditLvl = (EditLevel) target;

		// A plethora of buttons used to call the methods in the EditLevel class/
		// Do exactly what they says on the tin.
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Make New Level")) {
			((EditLevel)target).getNewMap();
		}
		if (GUILayout.Button("Save Level")) {
			((EditLevel)target).saveLevel();
			//Debug.Log("WTF!");	// right now this scripts isn't working right
		}
		if (GUILayout.Button("Load Level")) {
			((EditLevel)target).loadLevelByName();
			Debug.Log("WTF! v2");   // right now this scripts isn't working right
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		//playerLevel = GUILayout.TextField(playerLevel);
		GUILayout.Label("Level Name", GUILayout.Width(labelWidth));
		EditLvl.levelName = GUILayout.TextField(EditLvl.levelName);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Level Path", GUILayout.Width(labelWidth));
		EditLvl.levelPath = GUILayout.TextField(EditLvl.levelPath);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		EditLvl.overwriteLevel = GUILayout.Toggle(EditLvl.overwriteLevel, "Overwrite Level");
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);

		if (GUILayout.Button("Re-generate levels from scripts")) {
			BatchGenerate.GenerateRooms();
		}
		GUILayout.Space(10f);

		if (GUILayout.Button("Create New Tile-Chunk")) {
			((EditLevel)target).createTileChunk();
		}
		EditLvl.methodDirection = (GameManager.Direction)EditorGUILayout.EnumPopup("Direction", (System.Enum)EditLvl.methodDirection);
		GUILayout.BeginHorizontal();
		EditLvl.newChunkWidth = EditorGUILayout.IntField("Chunk Width", EditLvl.newChunkWidth);
		EditLvl.newChunkHeight = EditorGUILayout.IntField("Chunk Height", EditLvl.newChunkHeight);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		EditLvl.linkX = EditorGUILayout.IntSlider("X to link to", EditLvl.linkX, 0, EditLvl.newChunkWidth - 1);
		EditLvl.linkY = EditorGUILayout.IntSlider("Y to link to", EditLvl.linkY, 0, EditLvl.newChunkHeight - 1);
		GUILayout.EndHorizontal();
		EditLvl.newChunkColor = EditorGUILayout.ColorField("New Chunk Color", EditLvl.newChunkColor);
		GUILayout.Space(10f);

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Create link")) {
			((EditLevel)target).createLink();
		}
		if (GUILayout.Button("Create wall")) {
			((EditLevel)target).createWall();
			//Debug.Log("Error: createWall() does not exist right now");
		}
		if (GUILayout.Button("Delete tile")) {
			((EditLevel)target).deleteTile();
		}
		GUILayout.EndHorizontal();
		EditLvl.methodDirection = (GameManager.Direction)EditorGUILayout.EnumPopup("Direction", (System.Enum)EditLvl.methodDirection);
		EditLvl.linkToIndex = EditorGUILayout.IntField("Index to link", EditLvl.linkToIndex);
		EditLvl.linkDirectionality = (EditLevel.LinkDirectionality)EditorGUILayout.EnumPopup(
			"Link directionality", 
			(System.Enum)EditLvl.linkDirectionality);
		GUILayout.Space(10f);

		if (GUILayout.Button("Set type")) {
			((EditLevel)target).setType();
		}
		EditLvl.type = (Node.TileType)EditorGUILayout.EnumPopup("Tile type", (System.Enum)EditLvl.type);
		GUILayout.Space(10f);

		if (GUILayout.Button("Redraw Enviroment")) {
			((EditLevel)target).redraw();
		}
		GUILayout.Space(10f);

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Apply changes to node")) {
		    ((EditLevel)target).applyToNode();
	    }
		GUILayout.BeginVertical();
		if (GUILayout.Button("Sample node appearance")) {
			((EditLevel)target).sampleTile();
		}
		EditLvl.drawing = GUILayout.Toggle(EditLvl.drawing, "Toggle drawing");
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		// node goes here
		GUILayout.EndHorizontal();

		/*
		GUILayout.BeginHorizontal();
		GUILayout.EndHorizontal();
		*/

		base.OnInspectorGUI();
	}
}
