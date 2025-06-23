// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Testbed.Local
{
#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(AssetDefinition))]

	public class AssetDefinitionEditor : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var inputs = property.FindPropertyRelative("_inputs");
			
			EditorGUI.PropertyField(
				new Rect(
					position.x,
					position.y,
					position.width,
					EditorGUIUtility.singleLineHeight
				),
				property.FindPropertyRelative("_instanceId")
			);
			EditorGUI.PropertyField(
				new Rect(
					position.x,
					position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
					position.width,
					EditorGUIUtility.singleLineHeight
				),
				property.FindPropertyRelative("_catalogUri")
			);
			EditorGUI.PropertyField(
				new Rect(
					position.x,
					position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2,
					position.width,
					EditorGUIUtility.singleLineHeight
				),
				property.FindPropertyRelative("_blueprintId")
			);
			
			EditorGUI.LabelField(
				new Rect(
					position.x,
					position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3,
					position.width,
					EditorGUIUtility.singleLineHeight
				),
				"Inputs ---------------"
			);

			EditorGUI.LabelField(
				new Rect(
					position.x,
					position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4,
					position.width / 6,
					EditorGUIUtility.singleLineHeight
				),
				"Add:"
			);
			if (GUI.Button(
				new Rect(
					position.x + (position.width / 6),
					position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4,
					position.width / 6,
					EditorGUIUtility.singleLineHeight
				),
				"String"
			))
			{
				inputs.arraySize++;
				inputs.GetArrayElementAtIndex(inputs.arraySize - 1)
					.managedReferenceValue = new StringInput();
			}
			if (GUI.Button(
				new Rect(
					position.x + ((position.width / 6) * 2),
					position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4,
					position.width / 6,
					EditorGUIUtility.singleLineHeight
				),
				"Int"
			))
			{
				inputs.arraySize++;
				inputs.GetArrayElementAtIndex(inputs.arraySize - 1)
					.managedReferenceValue = new IntInput();
			}
			if (GUI.Button(
				new Rect(
					position.x + ((position.width / 6) * 3),
					position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4,
					position.width / 6,
					EditorGUIUtility.singleLineHeight
				),
				"Float"
			))
			{
				inputs.arraySize++;
				inputs.GetArrayElementAtIndex(inputs.arraySize - 1)
					.managedReferenceValue = new FloatInput();
			}
			if (GUI.Button(
				new Rect(
					position.x + ((position.width / 6) * 4),
					position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4,
					position.width / 6,
					EditorGUIUtility.singleLineHeight
				),
				"Bool"
			))
			{
				inputs.arraySize++;
				inputs.GetArrayElementAtIndex(inputs.arraySize - 1)
					.managedReferenceValue = new BoolInput();
			}
			if (GUI.Button(
				new Rect(
					position.x + ((position.width / 6) * 5),
					position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4,
					position.width / 6,
					EditorGUIUtility.singleLineHeight
				),
				"Object"
			))
			{
				inputs.arraySize++;
				inputs.GetArrayElementAtIndex(inputs.arraySize - 1)
					.managedReferenceValue = new ObjectInput();
			}
			
			for (var i = 0; i < inputs.arraySize; i++)
			{
				var input = inputs.GetArrayElementAtIndex(i);
				EditorGUI.PropertyField(
					new Rect(
						position.x + 8f,
						position.y +
						(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (5 + (i * 2)),
						position.width - 16f - EditorGUIUtility.singleLineHeight,
						EditorGUIUtility.singleLineHeight
					),
					input.FindPropertyRelative("_key")
				);
				if (GUI.Button(
					new Rect(
						position.x + position.width - EditorGUIUtility.singleLineHeight,
						position.y +
						(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (5 + (i * 2)),
						EditorGUIUtility.singleLineHeight,
						EditorGUIUtility.singleLineHeight
					),
					"-"
				))
				{
					inputs.DeleteArrayElementAtIndex(i);
					break;
				}
				EditorGUI.PropertyField(
					new Rect(
						position.x + 8f,
						position.y +
						(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (6 + (i * 2)),
						position.width - 16f - EditorGUIUtility.singleLineHeight,
						EditorGUIUtility.singleLineHeight
					),
					input.FindPropertyRelative("_value")
				);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var inputs = property.FindPropertyRelative("_inputs");
			return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (5 + (inputs.arraySize * 2));
		}
	}
#endif
	
	[Serializable]
	public class AssetDefinition
	{
		public string InstanceId => _instanceId;
		public string CatalogUri => _catalogUri;
		public string BlueprintId => _blueprintId;
		public IBlueprintInput[] Inputs => _inputs;

		[SerializeField] private string _instanceId;
		[SerializeField] private string _catalogUri;
		[SerializeField] private string _blueprintId;
		[SerializeReference] private IBlueprintInput[] _inputs;
	}
}