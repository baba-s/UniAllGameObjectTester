using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kogane
{
	public static class AllGameObjectTester
	{
		//==============================================================================
		// 定数(static readonly)
		//==============================================================================
		private static readonly Func<string, bool>           DEFAULT_IS_TARGET_SCENE_DELEGATE = _ => true;
		private static readonly Func<GameObjectData, string> DEFAULT_TO_OUTPUT_TEXT_DELEGATE  = x => $"{x.AssetPath},{x.HierarchyPath}";

		//==============================================================================
		// 関数(static)
		//==============================================================================
		public static void Test
		(
			[NotNull]   Func<GameObject, bool>       isValid,
			[CanBeNull] Func<string, bool>           isTargetScene = null,
			[CanBeNull] Func<GameObjectData, string> toOutputText  = null
		)
		{
			if ( isTargetScene == null )
			{
				isTargetScene = DEFAULT_IS_TARGET_SCENE_DELEGATE;
			}

			if ( toOutputText == null )
			{
				toOutputText = DEFAULT_TO_OUTPUT_TEXT_DELEGATE;
			}

			var setup = EditorSceneManager.GetSceneManagerSetup();

			var scenePaths = AssetDatabase
					.FindAssets( "t:Scene" )
					.Select( x => AssetDatabase.GUIDToAssetPath( x ) )
					.Where( x => x.StartsWith( "Assets/" ) )
					.Where( x => isTargetScene( x ) )
					.ToArray()
				;

			var sb = new StringBuilder();

			foreach ( var scenePath in scenePaths )
			{
				EditorSceneManager.OpenScene( scenePath );

				var sceneResultTexts = GetGameObjectsInCurrentScene()
						.Where( x => !isValid( x ) )
						.Select( x => new GameObjectData( x.scene.path, x ) )
						.OrderBy( x => x.AssetPath )
						.ThenBy( x => x.HierarchyPath )
						.Select( x => toOutputText( x ) )
						.ToArray()
					;

				foreach ( var x in sceneResultTexts )
				{
					sb.AppendLine( x );
				}
			}

			EditorSceneManager.RestoreSceneManagerSetup( setup );

			var prefabResultTexts = GetGameObjectsInAllPrefab()
					.Where( x => !isValid( x ) )
					.Select( x => new GameObjectData( AssetDatabase.GetAssetPath( x ), x ) )
					.OrderBy( x => x.AssetPath )
					.ThenBy( x => x.HierarchyPath )
					.Select( x => toOutputText( x ) )
					.ToArray()
				;

			foreach ( var x in prefabResultTexts )
			{
				sb.AppendLine( x );
			}

			var message = sb.ToString();

			if ( string.IsNullOrWhiteSpace( message ) ) return;

			Assert.Fail( message );
		}

		private static IEnumerable<GameObject> GetGameObjectsInCurrentScene()
		{
			return Resources
					.FindObjectsOfTypeAll<GameObject>()
					.Where( x => x.scene.isLoaded )
					.Where( x => x.hideFlags == HideFlags.None )
				;
		}

		private static IEnumerable<GameObject> GetGameObjectsInAllPrefab()
		{
			return AssetDatabase
					.FindAssets( "t:Prefab" )
					.Select( x => AssetDatabase.GUIDToAssetPath( x ) )
					.Where( x => x.StartsWith( "Assets/" ) )
					.Select( x => AssetDatabase.LoadAssetAtPath<GameObject>( x ) )
					.Where( x => x != null )
					.SelectMany( x => x.GetComponentsInChildren<Transform>( true ) )
					.Select( x => x.gameObject )
				;
		}

		private static string GetRootPath( this GameObject gameObject )
		{
			var path   = gameObject.name;
			var parent = gameObject.transform.parent;

			while ( parent != null )
			{
				path   = parent.name + "/" + path;
				parent = parent.parent;
			}

			return path;
		}

		//==============================================================================
		// クラス
		//==============================================================================
		/// <summary>
		/// ゲームオブジェクトの情報を管理するクラス
		/// </summary>
		public sealed class GameObjectData
		{
			//==========================================================================
			// プロパティ
			//==========================================================================
			public GameObject GameObject    { get; }
			public string     AssetPath     { get; }
			public string     HierarchyPath { get; }

			//==========================================================================
			// 関数
			//==========================================================================
			internal GameObjectData( string assetPath, GameObject gameObject )
			{
				GameObject    = gameObject;
				AssetPath     = assetPath;
				HierarchyPath = gameObject.GetRootPath();
			}
		}
	}
}