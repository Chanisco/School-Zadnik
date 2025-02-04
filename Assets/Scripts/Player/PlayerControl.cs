﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControl : MonoBehaviour {
	/*---Animations en audio---*/
//	private Animator animator;
	public AudioClip Playerhit1,Playerhit2,Playerhit3,PlayerDeath;

	/*--- PUBLICS ---*/
	//list
	public static List<PathNode> solvedPath = new List<PathNode>();
	/*--- END PUBLICS ---*/

	/*--- PRIVATES ---*/
	//int
	private int startIndex;
	private int endIndex;
	private int lastStartIndex;
	private int lastEndIndex;
	private int place;
	private int battleRoar;
	//gameobject
	private GameObject start;
	private GameObject end;

	//bool
	private bool pathDone;
	private bool reset;

	//list
	private List<PathNode> sources;
	private List<PathNode> fov;
	/*--- END PRIVATES ---*/

	void Start () {
		sources = HexGrid.sources;
		start = this.gameObject;
//		animator = GetComponent<Animator>();
		int initPos = Closest(sources, start.transform.position);
		transform.position = sources[initPos].transform.position;

		GlobalValues.player = this.gameObject;
	}

	void OnEnable () {
		GameControler.PlayerClick += GetPath;
		EnemyControl.HitPlayer += HitPlayer;
	}
	
	void OnDisable () {
		GameControler.PlayerClick -= GetPath;
		EnemyControl.HitPlayer -= HitPlayer;
	}

	private IEnumerator Move () {
		/*--- MOVEMENT ---*/
		/*List<PathNode> fieldOfView = PathNode.FieldOfView(start, 0.5f);

		foreach (PathNode hex in sources) {
			if (hex != null) {
				hex.gameObject.SetActive(false);
			}
		}

		foreach (PathNode hex in fieldOfView) {
			Debug.Log(fieldOfView.Count);
			if (hex != null) {
				hex.gameObject.SetActive(true);
			}
		}*/

		GlobalValues.playerMove = true;

		for (int i = 1; i < solvedPath.Count; i++) {
			if (solvedPath[i] != null) {
				/*if (solvedPath[i - 1] != null) {
					solvedPath[i].tag = GlobalValues.cellTag;
				}*/
				transform.position = solvedPath[i].transform.position;
				solvedPath[i].tag = GlobalValues.playerTag;

				yield return new WaitForSeconds(0.5f);

				solvedPath[i].tag = GlobalValues.cellTag;

				if (transform.position.y > 7f) {
					Application.LoadLevel("PeterTest");
				}
			
				if (transform.position.y < -7f) {
					Application.LoadLevel("PeterTest");
				}

				if (transform.position.x > 7f) {
					Application.LoadLevel("PeterTest");
				}

				if (transform.position.x < -7f) {
					Application.LoadLevel("PeterTest");
				}
			} else {
				return false;
			}
		}

		GlobalValues.playerMove = false;
	}

	private void HitPlayer () {
		/*--- ENEMY ATTACK ---*/
		if (GlobalValues.playerHP != 0) {
			GlobalValues.playerHP--;
			battleRoar = Random.Range(1,10);
			switch(battleRoar){
			case 1:
				AudioSource.PlayClipAtPoint(Playerhit1, transform.position, 1);
			break;
			case 2:
				AudioSource.PlayClipAtPoint(Playerhit2, transform.position, 1);
				break;

			default:
				AudioSource.PlayClipAtPoint(Playerhit3, transform.position, 1);
				break;

			}
			if(GlobalValues.playerHP == 0){
				GlobalValues.playerHP = 10;
				GlobalValues.screenStance = false;
				AudioSource.PlayClipAtPoint(PlayerDeath, transform.position, 1);
				Application.LoadLevel("lose");
			}
		}
	}

	private void GetPath () {
		end = GlobalValues.targetTile;

		while (!pathDone) {
			if (reset) {
				solvedPath.Clear();
				reset = false;
			}

			if (start == null) {
				Debug.LogWarning("No start point!");

				pathDone = true;
			}

			if (end == null) {
				Debug.LogWarning("No end point!");

				pathDone = true;
			}

			startIndex = Closest(sources, start.transform.position);
			endIndex = Closest(sources, end.transform.position);

			if (startIndex != lastStartIndex || endIndex != lastEndIndex) {
				reset = true;

				lastStartIndex = startIndex;
				lastEndIndex = endIndex;

				continue;
			}

			if (!pathDone) {
				solvedPath = AStar.CalculatePath(sources[lastStartIndex], sources[lastEndIndex]);
				//pathDone = true;
			}

			if (solvedPath == null || solvedPath.Count < 1) {
				Debug.LogWarning("Invalid path!");
				reset = true;

				break;
			}

			for (int i = 0; i < solvedPath.Count - 1; i++) {
				if (AStar.InvalidNode(solvedPath[i]) || AStar.InvalidNode(solvedPath[i + 1])) {
					reset = true;

					continue;
				}

				Debug.DrawLine(solvedPath[i].Position, solvedPath[i + 1].Position, Color.cyan * new Color(1.0f, 1.0f, 1.0f, 1.0f));
			}

			if (reset) {
				continue;
			}

			if (solvedPath != null) {
				StopCoroutine("Move");
				StartCoroutine("Move");

				pathDone = true;
			}
		}

		pathDone = false;
	}

	private static int Closest (List<PathNode> inNodes, Vector2 toPoint) {
		int closestIndex = 0;
		float minDistance = float.MaxValue;
		
		for (int i = 0; i < inNodes.Count; i++) {
			if (AStar.InvalidNode(inNodes[i])) continue;
			
			float thisDistance = Vector2.Distance(toPoint, inNodes[i].Position);
			
			if (thisDistance > minDistance) continue;
			
			minDistance = thisDistance;
			closestIndex = i;
		}
		
		return closestIndex;
	}
}