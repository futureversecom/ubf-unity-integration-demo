// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using System;
using Futureverse.UBF.Runtime.Resources;
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Futureverse.UBF.ExecutionController.Runtime.Settings
{
	internal static class ExecutionControllerSettingsProvider
	{
#if UNITY_EDITOR
		[SettingsProvider]
		public static SettingsProvider CreateMyCustomSettingsProvider()
		{
			var provider = new SettingsProvider("Project/UBFExecutionController", SettingsScope.Project)
			{
				label = "UBF Execution Controller",
				guiHandler = GUIHandler,
				keywords = new HashSet<string>(
					new[]
					{
						"AssetProfile",
						"Asset",
						"Profile",
						"Path",
						"UBF",
						"Execution",
						"Futureverse",
					}
				),
			};

			return provider;
		}
		
		private static ReorderableList s_reorderableList;
		private static SerializedObject s_serializedSettings;
		private static SerializedProperty s_listProperty;
		private static Type[] s_subTypes;

		private static void GUIHandler(string obj)
		{
			EditorGUI.BeginChangeCheck();
			s_serializedSettings ??= ExecutionControllerSettings.GetSerializedSettings();
			var useARProfilesProperty = s_serializedSettings.FindProperty("_useAssetRegisterProfiles");
			EditorGUILayout.PropertyField(useARProfilesProperty);
			if (!useARProfilesProperty.boolValue)
			{
				EditorGUILayout.PropertyField(s_serializedSettings.FindProperty("_assetProfilesPath"));
			}
			EditorGUILayout.PropertyField(s_serializedSettings.FindProperty("_syloResolverUri"));
			
			var caching = s_serializedSettings.FindProperty("_cacheType");
			EditorGUILayout.PropertyField(caching);
			if (caching.intValue == 2)
			{
				EditorGUILayout.PropertyField(s_serializedSettings.FindProperty("_cachePathOverride"));
			}
			
			EditorGUILayout.PropertyField(s_serializedSettings.FindProperty("_supportedVariants"));
			
			s_listProperty = s_serializedSettings.FindProperty("_downloaders");
			s_reorderableList ??= new ReorderableList(
				s_serializedSettings,
				s_listProperty,
				true,
				true,
				true,
				true
			)
			{
				elementHeightCallback = ElementHeightCallback,
				drawElementCallback = DrawElementCallback,
				onAddDropdownCallback = OnAddDropdownCallback,
				onRemoveCallback = OnRemoveCallback,
				onReorderCallback = OnReorderCallback,
				drawHeaderCallback = DrawHeaderCallback
			};
			
			s_reorderableList.DoLayoutList();
			
			if (EditorGUI.EndChangeCheck())
			{
				s_serializedSettings.ApplyModifiedProperties();
			}
		}

		private static void DrawHeaderCallback(Rect rect)
		{
			EditorGUI.LabelField(rect, "Downloaders");
		}

		private static void OnReorderCallback(ReorderableList list)
		{
			s_serializedSettings.ApplyModifiedProperties();
		}

		private static void OnRemoveCallback(ReorderableList list)
		{
			if (list.index >= 0 && list.index < s_listProperty.arraySize)
			{
				s_listProperty.DeleteArrayElementAtIndex(list.index);
			}

			s_serializedSettings.ApplyModifiedProperties();
		}

		private static void OnAddDropdownCallback(Rect buttonRect, ReorderableList list)
		{
			s_subTypes ??= AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => typeof(IDownloader).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
				.ToArray();
			
			var menu = new GenericMenu();
			foreach (var type in s_subTypes)
			{
				if (!TypeIsInList(type))
				{
					menu.AddItem(new GUIContent(type.Name), false, () =>
					{
						s_listProperty.arraySize++;
						s_listProperty.GetArrayElementAtIndex(s_listProperty.arraySize - 1)
							.managedReferenceValue = Activator.CreateInstance(type);
						s_serializedSettings.ApplyModifiedProperties();
					});
				}
			}

			menu.ShowAsContext();
		}

		private static bool TypeIsInList(Type type)
		{
			for (var i = 0; i < s_listProperty.arraySize; ++i)
			{
				if (s_listProperty.GetArrayElementAtIndex(i)
						.managedReferenceValue.GetType() ==
					type)
				{
					return true;
				}
			}

			return false;
		}

		private static void DrawElementCallback(
			Rect rect,
			int index,
			bool isActive,
			bool isFocused)
		{
			var element = s_listProperty.GetArrayElementAtIndex(index);
			var name = element.managedReferenceValue?.GetType()?.Name ?? "Unknown";
			EditorGUI.LabelField(rect, name);
		}

		private static float ElementHeightCallback(int index)
			=> EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
#endif
	}
}